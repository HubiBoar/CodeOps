using Definit.Configuration;
using Definit.Validation;
using Microsoft.Extensions.Configuration;
using OneOf;
using OneOf.Else;
using Success = OneOf.Types.Success;

namespace CodeOps.ConfigurationAsCode;

public sealed partial class ConfigAsCode
{
    private readonly Dictionary<string, OneOf<Value, Manual>> _entries = [];
    private readonly List<Func<IConfiguration, OneOf<Success, ValidationErrors>>> _validators = [];

    private readonly IConfigAsCodeProvider _provider;

    public ConfigAsCode(IConfigAsCodeProvider provider)
    {
        _provider = provider;
    }

    public void AddReference<TSection>(Entry<TSection> reference)
        where TSection : ISectionName
    {
        foreach(var (name, entry) in reference.Entries)
        {
            _entries.Add(name, entry);
        }

        _validators.Add(reference.Validation);
    }

    public async Task<OneOf<Success, IReadOnlyCollection<ValidationErrors>>> Upload()
    {
        var values = await _provider.GetValues();
        var errors = new List<ValidationErrors>();
        var newEntries = new Dictionary<string, string>();

        foreach (var (name, entry) in _entries)
        {
            entry.Match(
                value => value.Val,
                manual => GetExistingValue(values, name))
            .Switch(
                value => newEntries.Add(name, value),
                errors.Add);
        }

        if(errors.Count > 0)
        {
            return errors;
        }

        var result = ValidateValues(newEntries);

        if(result.Is(out Success success).Else(out var resultErrors))
        {
            await _provider.UploadValues(newEntries);

            var valuesAfterUpload = await _provider.GetValues();

            return ValidateValues(valuesAfterUpload);
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

    private static OneOf<string, ValidationErrors> GetExistingValue(IReadOnlyDictionary<string, string> values, string name)
    {
        if(values.TryGetValue(name, out var value))
        {
            return value;
        }
        else
        {
            return new ValidationErrors($"Entry: [{name}] NotFound in Values");
        }
    }
}