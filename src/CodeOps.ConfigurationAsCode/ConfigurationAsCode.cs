using System.Linq.Expressions;
using Definit.Validation;
using OneOf;
using Success = OneOf.Types.Success;

namespace CodeOps.ConfigurationAsCode;

public interface IConfigurationAsCodeProvider
{
    public Task<IReadOnlyDictionary<string, string>> GetValues();

    public Task UploadValues(IReadOnlyDictionary<string, string> entries);
}

public sealed class ConfigAsCode
{
    private sealed record Value(string Val);
    private sealed record Manual();

    private readonly Dictionary<string, (OneOf<Value, Manual> type, Validator<string>.Delegate validate)> _entries = [];
    private readonly IConfigurationAsCodeProvider _provider;

    public ConfigAsCode(IConfigurationAsCodeProvider provider)
    {
        _provider = provider;
    }

    public ConfigAsCode AddValue<TMethod>(string name, string value)
        where TMethod : IValidate<string>
    {
        _entries.Add(name, (new Value(value), TMethod.Validate));

        return this;
    }

    public ConfigAsCode AddManual<TMethod>(string name)
        where TMethod : IValidate<string>
    {
        _entries.Add(name, (new Manual(), TMethod.Validate));

        return this;
    }

    public async Task<OneOf<Success, IReadOnlyCollection<ValidationErrors>>> Upload()
    {
        var values = await _provider.GetValues();
        var errors = new List<ValidationErrors>();
        var newEntries = new Dictionary<string, string>();

        foreach (var (name, (entry, validate)) in _entries)
        {
            entry.Match(
                value => validate.InvokeAndReturnValue(value.Val),
                manual => ValidateExistingValue(values, name, validate))
            .Switch(
                value => newEntries.Add(name, value),
                errors.Add);
        }

        if(errors.Count > 0)
        {
            return errors;
        }

        await _provider.UploadValues(newEntries);

        return await ValidateExistingValues();
    }

    private async Task<OneOf<Success, IReadOnlyCollection<ValidationErrors>>> ValidateExistingValues()
    {
        var values = await _provider.GetValues();
        var errors = new List<ValidationErrors>();

        foreach(var (name, (entry, validate)) in _entries)
        {
            ValidateExistingValue(values, name, validate)
                .Switch(
                    _ => {},
                    errors.Add);
        }

        if(errors.Count > 0)
        {
            return errors;
        }

        return new Success();
    }

    private static OneOf<string, ValidationErrors> ValidateExistingValue(IReadOnlyDictionary<string, string> values, string name, Validator<string>.Delegate validate)
    {
        if(values.TryGetValue(name, out var value))
        {
            return validate.InvokeAndReturnValue(value);
        }
        else
        {
            return new ValidationErrors($"Entry: [{name}] NotFound in Values");
        }
    }
}