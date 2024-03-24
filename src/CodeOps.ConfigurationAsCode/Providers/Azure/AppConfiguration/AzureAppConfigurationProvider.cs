using Azure;
using Azure.ResourceManager.AppConfiguration;
using Azure.ResourceManager.AppConfiguration.Models;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.DependencyInjection;
using CodeOps.InfrastructureAsCode.Azure;
using Microsoft.Extensions.Configuration;
using AppConfig = CodeOps.ConfigurationAsCode.Azure.AzureAppConfigAsCodeSource;
using Azure.ResourceManager.Authorization;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.KeyVault;
using Azure.ResourceManager.Authorization.Models;

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
    private readonly Func<AzureDeploymentOptions, Task<SecretClient>> _getSecretsClient;
    private readonly Func<AzureDeploymentOptions, Task<SecretClient>> _provisionSecretsClient;
    private readonly Func<AzureDeploymentOptions, Uri, SecretClient, Task<AppConfig>> _factory;

    private AzureAppConfigurationProvider(
        Name name,
        Sku sku,
        IServiceCollection services,
        IConfigurationManager configuration,
        ConfigAsCode.Enabled enabled,
        Func<AzureDeploymentOptions, Uri, SecretClient, Task<AppConfig>> factory,
        Func<AzureDeploymentOptions, Task<SecretClient>> getSecretsClient,
        Func<AzureDeploymentOptions, Task<SecretClient>> provisionSecretsClient)
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

    public async Task<AppConfig> Get(AzureDeploymentOptions credentials)
    {
        var uri = GetAppConfigUri(_name);
        var secretClient = await _getSecretsClient(credentials);
        var source = await _factory(credentials, uri, secretClient);

        return source;
    }

    public async Task<AppConfig> Provision(AzureDeploymentOptions options)
    {
        var resourceGroup = options.ResourceGroup;
        var location = options.Location;
        var credentials = options.Credentials;

        var skuName = _sku switch
        {
            Sku.Free => "free",
            Sku.Standard => "standard",
            _ => "free"
        };
        
        var resourceData = new AppConfigurationStoreData(location, new AppConfigurationSku(skuName))
        {
            Identity = new ManagedServiceIdentity(ManagedServiceIdentityType.SystemAssigned)
        };

        var resources = resourceGroup.GetAppConfigurationStores();

        var result = await resources
            .CreateOrUpdateAsync(
                WaitUntil.Completed,
                _name.Value,
                resourceData);

        var uri = GetAppConfigUri(_name);
        var secretClient = await _provisionSecretsClient(options);

        var keyVault = options
            .Subscription
            .GetKeyVaults()
            .Single(x => x.Data.Properties.VaultUri == secretClient.VaultUri);

        var roleAssignmentName = "KeyVaultAdministrator";
        var roleAssignment = new RoleAssignmentCreateOrUpdateContent(keyVault.Data.Id, result.Value.Data.Identity.PrincipalId!.Value);

        var roles = keyVault.GetRoleAssignments();

        await roles.CreateOrUpdateAsync(
            WaitUntil.Completed,
            roleAssignmentName,
            roleAssignment);

        var source = await _factory(options, uri, secretClient);

        return source;
    }

    private static Uri GetAppConfigUri(Name name)
    {
        return new ($"https://{name.Value}.azconfig.io");   
    }

    async Task<ConfigAsCode.ISource> IAzureComponentProvider<ConfigAsCode.ISource>.Get(AzureDeploymentOptions options)
    {
        return await Get(options);
    }

    async Task<ConfigAsCode.ISource> IAzureComponentProvider<ConfigAsCode.ISource>.Provision(AzureDeploymentOptions options)
    {
        return await Provision(options);
    }

    async Task<ConfigAsCode.Builder> IAzureComponentProvider<ConfigAsCode.Builder>.Get(AzureDeploymentOptions options)
    {
        var source = await Get(options);
        return new ConfigAsCode.Builder(source, _enabled, _services, _configuration);
    }

    async Task<ConfigAsCode.Builder> IAzureComponentProvider<ConfigAsCode.Builder>.Provision(AzureDeploymentOptions options)
    {
        var source = await Provision(options);
        return new ConfigAsCode.Builder(source, _enabled, _services, _configuration);
    }
}