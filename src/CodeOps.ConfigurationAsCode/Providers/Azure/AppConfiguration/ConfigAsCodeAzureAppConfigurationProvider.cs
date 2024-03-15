using Azure.Core;
using Azure.Data.AppConfiguration;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using OneOf;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace CodeOps.ConfigurationAsCode.Providers.Azure;

public sealed class ConfigAsCodeAzureAppConfigurationProvider : IConfigAsCodeProvider
{
    private static readonly string Sentinel = Azure.Sentinel.Name;
    
    private readonly string? _label;
    private readonly Uri _appConfigUri;
    private readonly TokenCredential _credential;
    private readonly ConfigurationClient _configurationClient;
    private readonly SecretClient _keyVaultClient;

    public ConfigAsCodeAzureAppConfigurationProvider(
        TokenCredential credential,
        ConfigurationClient configurationClient,
        SecretClient keyVaultClient,
        Uri appConfigUri,
        string? label)
    {
        _label = label;
        _appConfigUri = appConfigUri;
        _credential = credential;
        _configurationClient = configurationClient;
        _keyVaultClient = keyVaultClient;
    }

    public Task DownloadValues(IConfigurationBuilder configuration)
    {
        var builder = new ConfigurationBuilder();
        builder.AddAzureAppConfiguration(options =>
        {
            options
                .Connect(_appConfigUri, _credential)
                .Select(keyFilter: KeyFilter.Any, labelFilter: _label)
                .ConfigureRefresh(refresh =>
                {
                    refresh.Register(Sentinel, _label, refreshAll: true);
                    refresh.SetCacheExpiration(TimeSpan.FromSeconds(5));
                })
                .ConfigureKeyVault(kv => kv.SetCredential(_credential))
                .UseFeatureFlags(flags =>
                {
                    flags.Label = _label;
                    flags.CacheExpirationInterval = TimeSpan.FromSeconds(5);
                });
        });

        var readonlyDictionary = (IReadOnlyDictionary<string, string>)builder.Build().AsEnumerable().ToDictionary()!;

        return Task.FromResult(readonlyDictionary);
    }

    public Task UploadValues(IReadOnlyDictionary<string, OneOf<ConfigAsCode.Value, ConfigAsCode.Reference>> entries)
    {
        foreach(var (name, entry) in entries)
        {
            entry.Switch(
                value =>
                {
                    _configurationClient.SetConfigurationSetting(new ConfigurationSetting(name, value.Val, _label));
                }, 
                reference =>
                {
                    var secretId = $"{_keyVaultClient.VaultUri}/{reference.Path}";
                    var secretReferenceSetting = new SecretReferenceConfigurationSetting(name, new Uri(secretId), _label);
                    _configurationClient.SetConfigurationSetting(secretReferenceSetting);
                });
        }

        return Task.CompletedTask;
    }

    public Task<bool> TryGetReference(string path, out string value)
    {
        var setting = _configurationClient.GetConfigurationSetting(path, _label);

        if(setting is not null)
        {
            value = setting.Value.Value;
            return Task.FromResult(true);
        }
        else
        {
            value = string.Empty;
            return Task.FromResult(false);
        }
    }
}