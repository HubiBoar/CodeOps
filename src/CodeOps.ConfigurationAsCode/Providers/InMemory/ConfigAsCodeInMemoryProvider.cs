using Microsoft.Extensions.Configuration;
using OneOf;

namespace CodeOps.ConfigurationAsCode.Providers;

public sealed class ConfigAsCodeInMemoryProvider : IConfigAsCodeProvider
{
    private Dictionary<string, string> _values;

    public ConfigAsCodeInMemoryProvider(IConfiguration configuration)
    {
        _values = configuration.AsEnumerable().ToDictionary()!;
    }

    public Task DownloadValues(IConfigurationBuilder builder)
    {
        builder.AddInMemoryCollection(_values!);
        return Task.CompletedTask;
    }

    public Task UploadValues(IReadOnlyDictionary<string, OneOf<ConfigAsCode.Value, ConfigAsCode.Reference>> entries)
    {
        Dictionary<string, string>  newValues = [];

        foreach(var (name, entry) in entries)
        {
            var value = entry.Match(
                value => value.Val,
                reference => _values[reference.Path]);

            newValues.Add(name, value);
        }

        _values.Clear();
        _values = newValues;

        return Task.CompletedTask;
    }

    public Task<bool> TryGetReference(string path, out string value)
    {
        return Task.FromResult(_values.TryGetValue(path, out value!));
    }
}