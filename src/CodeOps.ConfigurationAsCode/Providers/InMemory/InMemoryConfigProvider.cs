using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneOf;

namespace CodeOps.ConfigurationAsCode;

public sealed class InMemoryConfigAsCodeSource : ConfigAsCode.ISource
{
    private Dictionary<string, string> _values;

    public InMemoryConfigAsCodeSource(IConfiguration configuration)
    {
        _values = configuration.AsEnumerable().ToDictionary()!;
    }

    public void UploadValues(
        IReadOnlyDictionary<
            ConfigAsCode.Path,
            OneOf<
                ConfigAsCode.Value, 
                ConfigAsCode.FiltersEnabledFor,
                ConfigAsCode.FeatureFlag,
                ConfigAsCode.Reference>>
        entries)
    {
        Dictionary<ConfigAsCode.Path, ConfigAsCode.Reference> references = [];

        _values = entries
            .Select((path, entry) => (
                path.Value,
                entry.Match(
                    value     => value.Val,
                    filters   => filters.ToJson(),
                    flag      => flag.ToJson(),
                    reference =>
                    {
                        references.Add(path, reference);
                        return string.Empty;
                    })))
            .ToDictionary();

        foreach(var (path, reference) in references)
        {
            _values[path.Value] = _values[reference.Path.Value];
        }
    }

    public string GetValue(ConfigAsCode.Path path)
    {
        if(_values.TryGetValue(path.Value, out var value))
        {
            return value;
        }

        return string.Empty;
    }

    public void Register(IServiceCollection services, IConfigurationManager config)
    {
        config.AddInMemoryCollection(_values!);
    }
}