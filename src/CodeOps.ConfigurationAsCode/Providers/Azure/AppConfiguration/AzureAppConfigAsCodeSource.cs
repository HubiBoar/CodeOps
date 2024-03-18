using Azure.Core;
using Azure.Data.AppConfiguration;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using OneOf;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace CodeOps.ConfigurationAsCode.Azure;

public sealed class AzureAppConfigAsCodeSource : ConfigAsCode.ISource
{
    private readonly string? _label;
    private readonly Uri _appConfigUri;
    private readonly TokenCredential _credential;
    private readonly ConfigurationClient _configurationClient;
    private readonly SecretClient _keyVaultClient;

    private AzureAppConfigAsCodeSource(
        TokenCredential credential,
        SecretClient keyVaultClient,
        Uri appConfigUri,
        string? label)
    {
        _label = label;
        _appConfigUri = appConfigUri;
        _credential = credential;
        _configurationClient = new ConfigurationClient(appConfigUri, credential);
        _keyVaultClient = keyVaultClient;
    }

    public static AzureAppConfigAsCodeSource Create(
        TokenCredential credential,
        SecretClient keyVaultClient,
        Uri appConfigUri,
        string? label,
        IServiceCollection services,
        IConfigurationManager config,
        ConfigAsCode.Enabled enabled,
        ConfigAsCode.IEntry<Sentinel> sentinel)
    {
        var provider = new AzureAppConfigAsCodeSource(credential, keyVaultClient, appConfigUri, label);

        provider.AddConfig(Sentinel.Register, services, config, enabled, sentinel);

        return provider;
    }

    public static AzureAppConfigAsCodeSource Create(
        TokenCredential credential,
        SecretClient keyVaultClient,
        Uri appConfigUri,
        string? label,
        IServiceCollection services,
        IConfigurationManager config,
        ConfigAsCode.Enabled enabled,
        ConfigAsCode.IEntry<Sentinel> sentinel,
        ConfigAsCode.IEntry<ConfigurationRefresherDelay> refresher)
    {
        var provider = new AzureAppConfigAsCodeSource(credential, keyVaultClient, appConfigUri, label);

        provider.AddConfig(Sentinel.Register, services, config, enabled, sentinel);
        provider.AddConfig(ConfigurationRefresherDelay.Register, services, config, enabled, refresher);

        services.AddHostedService<AppConfigurationRefresher>();

        return provider;
    }

    public void Register(IServiceCollection services, IConfigurationManager config)
    {
        config.AddAzureAppConfiguration(options =>
        {
            options
                .Connect(_appConfigUri, _credential)
                .Select(keyFilter: KeyFilter.Any, labelFilter: _label)
                .ConfigureRefresh(refresh =>
                {
                    refresh.Register(Sentinel.Name, _label, refreshAll: true);
                    refresh.SetCacheExpiration(TimeSpan.FromSeconds(5));
                })
                .ConfigureKeyVault(options => options.Register(_keyVaultClient))
                .UseFeatureFlags(flags =>
                {
                    flags.Label = _label;
                    flags.CacheExpirationInterval = TimeSpan.FromSeconds(5);
                });
        });

        services.AddAzureAppConfiguration();
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

        foreach(var (path, entry) in entries)
        {
            entry.Switch(
                value => _configurationClient.SetConfigurationSetting(new ConfigurationSetting(path.Value, value.Val, _label)),
                filter =>
                {
                    var flagSetting = new FeatureFlagConfigurationSetting(path.Value, true, _label);

                    foreach(var f in filter.EnabledFor)
                    {
                        var json = JsonSerializer
                            .Serialize(f.Parameters);

                        var parameters = JsonSerializer
                            .Deserialize<Dictionary<string, string>>(json)?
                            .Select(x => (x.Key, (object)x.Value))
                            .ToDictionary();

                        flagSetting.ClientFilters.Add(new FeatureFlagFilter(f.Name, parameters));
                    }

                    _configurationClient.SetConfigurationSetting(flagSetting);
                },
                flag =>
                {
                    var flagSetting = new FeatureFlagConfigurationSetting(path.Value, flag.Enabled, _label);
                    _configurationClient.SetConfigurationSetting(flagSetting);
                }, 
                reference => references.Add(path, reference));
        }

        foreach(var (path, reference) in references)
        {
            var secretId = $"{_keyVaultClient.VaultUri}/{reference.Path}";
            var secretReferenceSetting = new SecretReferenceConfigurationSetting(path.Value, new Uri(secretId), _label);
            _configurationClient.SetConfigurationSetting(secretReferenceSetting);
        }
    }

    public string GetValue(ConfigAsCode.Path path)
    {
        var setting = _configurationClient.GetConfigurationSetting(path.Value, _label);

        return setting.Value.Value ?? string.Empty;
    }
}