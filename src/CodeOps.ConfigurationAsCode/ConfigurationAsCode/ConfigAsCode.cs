using CodeOps.InfrastructureAsCode;
using Definit.Results;
using Definit.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneOf.Else;

namespace CodeOps.ConfigurationAsCode;

public static partial class ConfigAsCode
{
    public sealed class Builder : InfraAsCode.IComponent
    {
        private readonly Enabled _enabled;
        private readonly IServiceCollection _services;
        private readonly IConfigurationManager _config;
        private readonly ISource _source;

        public Builder(ISource source, Enabled enabled, IServiceCollection services, IConfigurationManager config)
        {
            _source = source;
            _enabled = enabled;
            _services = services;
            _config = config;

            _source.Register(services, _config);
        }
        
        public ValidationResult AddConfig<TOptions>(RegisterConfig factory, IEntry<TOptions> configAsCode)
        {
            return ConfigAsCode.AddConfig(factory, _services, _config, _enabled, _source , configAsCode);
        }
    }

    public static ValidationResult AddConfig<TOptions>(
        this ISource provider,
        RegisterConfig factory,
        IServiceCollection services,
        IConfigurationManager config,
        Enabled enabled,
        IEntry<TOptions> configAsCode)
    {
        return AddConfig(factory, services, config, enabled, provider, configAsCode);
    }

    public static ValidationResult AddConfig<TOptions>(
        RegisterConfig factory,
        IServiceCollection services,
        IConfigurationManager config,
        Enabled enabled,
        ISource provider,
        IEntry<TOptions> configAsCode)
    {
        if(enabled.IsEnabled == false)
        {
            return factory(services, config);
        }

        if(AddEntry(provider, configAsCode)
            .Is(out Error error)
            .Else(out var remainder))
        {
            return error;
        }
        if(remainder
            .Is(out ValidationErrors validationErrors)
            .Else(out Success _))
        {
            return validationErrors;
        }
        
        provider.Register(services, config);

        return factory(services, config);
    }

    public static ValidationResult AddEntry<TOptions>(ISource provider, IEntry<TOptions> configAsCode)
    {
        var newEntry = configAsCode.ConfigurationAsCode(new Context<TOptions>());

        var values = newEntry
            .Entries
            .Select((path, entry) => (
                path.Value,
                entry.Match(
                    value => value.Val,
                    filter => filter.ToJson(),
                    flag => flag.ToJson(),
                    manual => provider.GetValue(path),
                    reference => provider.GetValue(reference.Path))))
            .ToDictionary();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(values!)
            .Build();

        return newEntry.Validation(config);
    }
}

public static class DictionaryExtensions
{
    public static IEnumerable<TResult> Select<TKey, TValue, TResult>(this IEnumerable<KeyValuePair<TKey, TValue>> value, Func<TKey, TValue, TResult> func)
    {
        return value.Select(val => func(val.Key, val.Value));
    }
}