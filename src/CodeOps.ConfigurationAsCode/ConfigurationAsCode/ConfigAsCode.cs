using Definit.Configuration;
using Definit.Validation;
using Microsoft.Extensions.Configuration;
using OneOf;
using OneOf.Else;
using Success = OneOf.Types.Success;

namespace CodeOps.ConfigurationAsCode;

public sealed partial class ConfigAsCode
{
    private readonly Dictionary<string, OneOf<Value, Manual, Reference>> _entries = [];
    private readonly List<Func<IConfiguration, OneOf<Success, ValidationErrors>>> _validators = [];

    private readonly IConfigAsCodeProvider _provider;

    public ConfigAsCode(IConfigAsCodeProvider provider)
    {
        _provider = provider;
    }

    public void AddEntries<TSection>(Entry<TSection> newEntry)
        where TSection : ISectionName
    {
        foreach(var (name, entry) in newEntry.Entries)
        {
            _entries.Add(name, entry);
        }

        _validators.Add(newEntry.Validation);
    }

    public async Task<OneOf<Success, IReadOnlyCollection<ValidationErrors>>> Upload()
    {
        var builder = new ConfigurationBuilder();
        var errors = new List<ValidationErrors>();
        var valuesToBeChecked = new Dictionary<string, string>();
        var newEntries = new Dictionary<string, OneOf<Value, Reference>>();

        await _provider.DownloadValues(builder);
        var values = builder.Build().AsEnumerable().ToDictionary();

        foreach (var (name, entry) in _entries)
        {
            if(entry.Is(out Value value).Else(out var rest))
            {
                valuesToBeChecked.Add(name, value.Val);
                newEntries.Add(name, value);
            }
            else if(rest.Is(out Manual _).Else(out var reference))
            {
                if(values.TryGetValue(name, out var foundValue))
                {
                    valuesToBeChecked.Add(name, foundValue!);
                    newEntries.Add(name, new Value(foundValue!));
                }
                else
                {
                    errors.Add(new ValidationErrors($"Entry: [{name}] NotFound in Values"));
                }
            }
            else
            {
                if(await _provider.TryGetReference(reference.Path, out var referenceValue))
                {
                    valuesToBeChecked.Add(name, referenceValue);
                    newEntries.Add(name, reference);
                }
                else
                {
                    errors.Add(new ValidationErrors($"Entry: [{name}] Reference NotFound"));
                }
            }
        }

        if(errors.Count > 0)
        {
            return errors;
        }

        var result = ValidateValues(valuesToBeChecked);

        if(result.Is(out Success success).Else(out var resultErrors))
        {
            await _provider.UploadValues(newEntries);

            return success;
        }

        return resultErrors.ToList();
    }

    private OneOf<Success, IReadOnlyCollection<ValidationErrors>> ValidateValues(IReadOnlyDictionary<string, string> values)
    {
        var errors = new List<ValidationErrors>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values!)
            .Build();

        foreach(var validator in _validators)
        {
            validator(configuration).Switch(success => {}, errors.Add);
        }

        if(errors.Count > 0)
        {
            return errors;
        }

        return new Success();
    }
}