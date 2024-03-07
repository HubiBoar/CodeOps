using System.Linq.Expressions;
using Definit.Configuration;
using Definit.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using OneOf;
using OneOf.Else;
using Success = OneOf.Types.Success;

namespace CodeOps.ConfigurationAsCode;

public interface IConfigurationAsCodeProvider
{
    public Task<IReadOnlyDictionary<string, string>> GetValues();

    public Task UploadValues(IReadOnlyDictionary<string, string> entries);
}

public sealed class ConfigAsCode
{
    public interface IProvider<TSection>
        where TSection : ISectionName
    {
        public Entry<TSection> ConfigurationAsCode(Context<TSection> context);
    }

    public sealed class Context<TSection>
        where TSection : ISectionName
    {
        internal Context()
        {
        }
    }

    public sealed record Entry<TSection>(
        Dictionary<string, OneOf<Value, Manual>> Entries,
        Func<IConfiguration, OneOf<Success, ValidationErrors>> Validation)
        where TSection : ISectionName
    {
        public Entry(
            string sectionName,
            OneOf<Value, Manual> sectionValue,
            Func<IConfiguration, OneOf<Success, ValidationErrors>> validation) :
            this(new Dictionary<string, OneOf<Value, Manual>>{[sectionName] = sectionValue}, validation)
        {
        }
    }

    public sealed record Value(string Val);
    public sealed record Manual();
    public sealed record Reference(string Path);

    private readonly Dictionary<string, OneOf<Value, Manual>> _entries = [];
    private readonly List<Func<IConfiguration, OneOf<Success, ValidationErrors>>> _validators = [];

    private readonly IConfigurationAsCodeProvider _provider;

    public ConfigAsCode(IConfigurationAsCodeProvider provider)
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