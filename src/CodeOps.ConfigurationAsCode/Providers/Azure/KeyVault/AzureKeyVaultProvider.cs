using Azure;
using Azure.Security.KeyVault.Secrets;
using CodeOps.InfrastructureAsCode.Azure;
using Azure.ResourceManager.KeyVault.Models;
using Azure.ResourceManager.KeyVault;
using KeyVault = CodeOps.ConfigurationAsCode.Azure.AzureKeyVaultConfigAsCodeSource;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

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

    public static SecretClient GetKeyVault(Name name, AzureCredentails credentials)
    {
        var keyVaultUri = GetKeyVaultUri(name);
        return new SecretClient(keyVaultUri, credentials.Credentials);
    }
    
    public static SecretClient ProvisionKeyVault(Name name, Sku sku, AzureCredentails credentials, AzureDeploymentOptions options)
    {
        var subscription = options.Subscription;
        var location = options.Location;
        var resourceGroup = options.ResourceGroup;
        
        var properties = new KeyVaultProperties(subscription.Data.TenantId!.Value, new KeyVaultSku(KeyVaultSkuFamily.A, Map(sku)))
        {
            EnabledForTemplateDeployment = true,
            EnableRbacAuthorization = true
        };
       
        var resourceData = new KeyVaultCreateOrUpdateContent(location, properties);

        var resources = resourceGroup.GetKeyVaults();
        if (resources.Exists(name.Value) == false)
        {
            resources.CreateOrUpdate(
                WaitUntil.Completed,
                name.Value,
                resourceData);
        }

        return GetKeyVault(name, credentials);
    }

    public KeyVault Get(AzureCredentails credentials)
    {
        var secretClient = GetKeyVault(_name, credentials);

        return new KeyVault(secretClient);
    }

    public KeyVault Provision(AzureCredentails credentials, AzureDeploymentOptions options)
    {
        var secretClient = ProvisionKeyVault(_name, _sku, credentials, options);

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