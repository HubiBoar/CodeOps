using Azure;
using Azure.ResourceManager.AppConfiguration;
using Azure.ResourceManager.AppConfiguration.Models;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.DependencyInjection;
using CodeOps.InfrastructureAsCode.Azure;
using Microsoft.Extensions.Configuration;
using AppConfig = CodeOps.ConfigurationAsCode.Azure.AzureAppConfigAsCodeSource;

namespace CodeOps.ConfigurationAsCode.Azure;

public sealed partial  class AzureAppConfigurationProvider : IAzureComponentProvider<AppConfig>, IAzureComponentProvider<ConfigAsCode.ISource>, IAzureComponentProvider<ConfigAsCode.Builder>
{
    public sealed record Name(string Value);

    public enum Sku
    {
        Free,
        Standard
    }

    public sealed record Label(string? Value = null);

    private readonly Name _name;
    private readonly Sku _sku;
    private readonly IServiceCollection _services;
    private readonly IConfigurationManager _configuration;
    private readonly ConfigAsCode.Enabled _enabled;
    private readonly Func<AzureCredentails, SecretClient> _getSecretsClient;
    private readonly Func<AzureCredentails, AzureDeploymentOptions, SecretClient> _provisionSecretsClient;
    private readonly Func<AzureCredentails, Uri, SecretClient, AppConfig> _factory;

    private AzureAppConfigurationProvider(
        Name name,
        Sku sku,
        IServiceCollection services,
        IConfigurationManager configuration,
        ConfigAsCode.Enabled enabled,
        Func<AzureCredentails, Uri, SecretClient, AppConfig> factory,
        Func<AzureCredentails, SecretClient> getSecretsClient,
        Func<AzureCredentails, AzureDeploymentOptions, SecretClient> provisionSecretsClient)
    {
        _name = name;
        _sku = sku;
        _services = services;
        _configuration = configuration;
        _enabled = enabled;
        _factory = factory;
        _getSecretsClient = getSecretsClient;
        _provisionSecretsClient = provisionSecretsClient;
    }

    public AppConfig Get(AzureCredentails credentials)
    {
        var uri = GetAppConfigUri(_name);
        var secretClient = _getSecretsClient(credentials);
        var source = _factory(credentials, uri, secretClient);

        return source;
    }

    public AppConfig Provision(AzureCredentails credentials, AzureDeploymentOptions options)
    {
        var resourceGroup = options.ResourceGroup;
        var location = options.Location;

        var skuName = _sku switch
        {
            Sku.Free => "free",
            Sku.Standard => "standard",
            _ => "free"
        };
        
        var resourceData = new AppConfigurationStoreData(location, new AppConfigurationSku(skuName));

        var resources = resourceGroup.GetAppConfigurationStores();
        if (resources.Exists(_name.Value) == false)
        {
            resources.CreateOrUpdate(
                WaitUntil.Completed,
                _name.Value,
                resourceData);
        }

        var uri = GetAppConfigUri(_name);
        var secretClient = _provisionSecretsClient(credentials, options);
        var source = _factory(credentials, uri, secretClient);

        return source;
    }

    private static Uri GetAppConfigUri(Name name)
    {
        return new ($"https://{name.Value}.azconfig.io");   
    }

    ConfigAsCode.ISource IAzureComponentProvider<ConfigAsCode.ISource>.Get(AzureCredentails credentials)
    {
        return Get(credentials);
    }

    ConfigAsCode.ISource IAzureComponentProvider<ConfigAsCode.ISource>.Provision(AzureCredentails credentials, AzureDeploymentOptions options)
    {
        return Provision(credentials, options);
    }

    ConfigAsCode.Builder IAzureComponentProvider<ConfigAsCode.Builder>.Get(AzureCredentails credentials)
    {
        var source = Get(credentials);
        return new ConfigAsCode.Builder(source, _enabled, _services, _configuration);
    }

    ConfigAsCode.Builder IAzureComponentProvider<ConfigAsCode.Builder>.Provision(AzureCredentails credentials, AzureDeploymentOptions options)
    {
        var source = Provision(credentials, options);
        return new ConfigAsCode.Builder(source, _enabled, _services, _configuration);
    }
}