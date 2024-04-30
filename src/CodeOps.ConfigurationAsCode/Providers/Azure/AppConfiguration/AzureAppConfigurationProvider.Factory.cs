using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using KeyVault = CodeOps.ConfigurationAsCode.Azure.AzureKeyVaultProvider;
using AppConfig = CodeOps.ConfigurationAsCode.Azure.AzureAppConfigAsCodeSource;
using CodeOps.EnvironmentAsCode;
using CodeOps.ArgumentAsCode;

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
            (options, uri, secretClient) =>
                AppConfig.Create(
                    options.Credentials,
                    secretClient,
                    uri,
                    label.Value,
                    services,
                    configuration,
                    enabled,
                    sentinel).AsTask(),
            (options) => KeyVault.GetKeyVault(keyVaultName, options.Credentials).AsTask(),
            (options) => KeyVault.ProvisionKeyVault(keyVaultName, keyVaultSku, options));
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
            (options, uri, secretClient) =>
                AppConfig.Create(
                    options.Credentials,
                    secretClient,
                    uri,
                    label.Value,
                    services,
                    configuration,
                    enabled,
                    sentinel,
                    refresher).AsTask(),
            (options) => KeyVault.GetKeyVault(keyVaultName, options.Credentials).AsTask(),
            (options) => KeyVault.ProvisionKeyVault(keyVaultName, keyVaultSku, options));
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
            (options, uri, secretClient) =>
                AppConfig.Create(
                    options.Credentials,
                    secretClient,
                    uri,
                    label.Value,
                    services,
                    configuration,
                    enabled,
                    sentinel).AsTask(),
            (options) => secretClient.AsTask(),
            (options) => secretClient.AsTask());
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
            (options, uri, secretClient) =>
                AppConfig.Create(
                    options.Credentials,
                    secretClient,
                    uri,
                    label.Value,
                    services,
                    configuration,
                    enabled,
                    sentinel,
                    refresher).AsTask(),
            (options) => secretClient.AsTask(),
            (options) => secretClient.AsTask());
    }
}