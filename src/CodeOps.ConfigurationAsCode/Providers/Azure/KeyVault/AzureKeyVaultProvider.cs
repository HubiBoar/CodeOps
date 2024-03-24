using Azure;
using Azure.Security.KeyVault.Secrets;
using CodeOps.InfrastructureAsCode.Azure;
using Azure.ResourceManager.KeyVault.Models;
using Azure.ResourceManager.KeyVault;
using KeyVault = CodeOps.ConfigurationAsCode.Azure.AzureKeyVaultConfigAsCodeSource;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Azure.Core;
using CodeOps.EnvironmentAsCode;

namespace CodeOps.ConfigurationAsCode.Azure;

public sealed class AzureKeyVaultProvider : IAzureComponentProvider<KeyVault>, IAzureComponentProvider<ConfigAsCode.ISource>, IAzureComponentProvider<ConfigAsCode.Builder>
{
    public enum Sku
    {
        Standard,
        Premium
    }

    public sealed record Name(string Value);

    private readonly ConfigAsCode.Enabled _enabled;
    private readonly Name _name;
    private readonly Sku _sku;
    private readonly IServiceCollection _services;
    private readonly IConfigurationManager _configuration;

    public AzureKeyVaultProvider(ConfigAsCode.Enabled enabled, Name name, Sku sku, IServiceCollection services, IConfigurationManager configuration)
    {
        _enabled = enabled;
        _name = name;
        _sku = sku;
        _services = services;
        _configuration = configuration;
    }

    public static SecretClient GetKeyVault(Name name, TokenCredential credential)
    {
        var keyVaultUri = GetKeyVaultUri(name);
        return new SecretClient(keyVaultUri, credential);
    }
    
    public static async Task<SecretClient> ProvisionKeyVault(Name name, Sku sku, AzureDeploymentOptions options)
    {
        var subscription = options.Subscription;
        var location = options.Location;
        var resourceGroup = options.ResourceGroup;
        var credentials = options.Credentials;
        
        var properties = new KeyVaultProperties(subscription.Data.TenantId!.Value, new KeyVaultSku(KeyVaultSkuFamily.A, Map(sku)))
        {
            EnabledForTemplateDeployment = true,
            EnableRbacAuthorization = true
        };
       
        var resourceData = new KeyVaultCreateOrUpdateContent(location, properties);

        var resources = resourceGroup.GetKeyVaults();

        await resources.CreateOrUpdateAsync(
            WaitUntil.Completed,
            name.Value,
            resourceData);

        return GetKeyVault(name, credentials);
    }

    public Task<KeyVault> Get(AzureDeploymentOptions options)
    {
        var secretClient = GetKeyVault(_name, options.Credentials);

        return new KeyVault(secretClient).AsTask();
    }

    public async Task<KeyVault> Provision(AzureDeploymentOptions options)
    {
        var secretClient = await ProvisionKeyVault(_name, _sku, options);

        return new KeyVault(secretClient);
    }
    
    private static Uri GetKeyVaultUri(Name name)
    {
        return new Uri($"https://{name.Value}.vault.azure.net/");
    }

    private static KeyVaultSkuName Map(Sku sku)
    {
        return sku switch
        {
            Sku.Standard => KeyVaultSkuName.Standard,
            Sku.Premium => KeyVaultSkuName.Premium,
            _ => KeyVaultSkuName.Standard
        };
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