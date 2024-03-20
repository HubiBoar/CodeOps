using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using KeyVault = CodeOps.ConfigurationAsCode.Azure.AzureKeyVaultProvider;
using AppConfig = CodeOps.ConfigurationAsCode.Azure.AzureAppConfigAsCodeSource;

namespace CodeOps.ConfigurationAsCode.Azure;

public sealed partial class AzureAppConfigurationProvider
{
    public static AzureAppConfigurationProvider Create(
        Name name,
        Label label,
        Sku sku,
        KeyVault.Name keyVaultName,
        KeyVault.Sku keyVaultSku,
        ConfigAsCode.Enabled enabled,
        IServiceCollection services,
        IConfigurationManager configuration,
        ConfigAsCode.IEntry<Sentinel> sentinel)
    {
        return new AzureAppConfigurationProvider(
            name,
            sku,
            services,
            configuration,
            enabled,
            (credentials, uri, secretClient) =>
                AppConfig.Create(
                    credentials.Credentials,
                    secretClient,
                    uri,
                    label.Value,
                    services,
                    configuration,
                    enabled,
                    sentinel),
            (credentials) => KeyVault.GetKeyVault(keyVaultName, credentials),
            (credentials, options) => KeyVault.ProvisionKeyVault(keyVaultName, keyVaultSku, credentials, options));
    }

    public static AzureAppConfigurationProvider Create(
        Name name,
        Label label,
        Sku sku,
        KeyVault.Name keyVaultName,
        KeyVault.Sku keyVaultSku,
        ConfigAsCode.Enabled enabled,
        IServiceCollection services,
        IConfigurationManager configuration,
        ConfigAsCode.IEntry<Sentinel> sentinel,
        ConfigAsCode.IEntry<ConfigurationRefresherDelay> refresher)
    {
        return new AzureAppConfigurationProvider(
            name,
            sku,
            services,
            configuration,
            enabled,
            (credentials, uri, secretClient) =>
                AppConfig.Create(
                    credentials.Credentials,
                    secretClient,
                    uri,
                    label.Value,
                    services,
                    configuration,
                    enabled,
                    sentinel,
                    refresher),
            (credentials) => KeyVault.GetKeyVault(keyVaultName, credentials),
            (credentials, options) => KeyVault.ProvisionKeyVault(keyVaultName, keyVaultSku, credentials, options));
    }

    public static AzureAppConfigurationProvider Create(
        Name name,
        Label label,
        Sku sku,
        SecretClient secretClient,
        ConfigAsCode.Enabled enabled,
        IServiceCollection services,
        IConfigurationManager configuration,
        ConfigAsCode.IEntry<Sentinel> sentinel)
    {
        return new AzureAppConfigurationProvider(
            name,
            sku,
            services,
            configuration,
            enabled,
            (credentials, uri, secretClient) =>
                AppConfig.Create(
                    credentials.Credentials,
                    secretClient,
                    uri,
                    label.Value,
                    services,
                    configuration,
                    enabled,
                    sentinel),
            (credentials) => secretClient,
            (credentials, options) => secretClient);
    }

    public static AzureAppConfigurationProvider Create(
        Name name,
        Label label,
        Sku sku,
        SecretClient secretClient,
        ConfigAsCode.Enabled enabled,
        IServiceCollection services,
        IConfigurationManager configuration,
        ConfigAsCode.IEntry<Sentinel> sentinel,
        ConfigAsCode.IEntry<ConfigurationRefresherDelay> refresher)
    {
        return new AzureAppConfigurationProvider(
            name,
            sku,
            services,
            configuration,
            enabled,
            (credentials, uri, secretClient) =>
                AppConfig.Create(
                    credentials.Credentials,
                    secretClient,
                    uri,
                    label.Value,
                    services,
                    configuration,
                    enabled,
                    sentinel,
                    refresher),
            (credentials) => secretClient,
            (credentials, options) => secretClient);
    }
}