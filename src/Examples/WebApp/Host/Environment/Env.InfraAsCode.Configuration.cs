using CodeOps.ArgumentAsCode;
using CodeOps.ConfigurationAsCode;
using CodeOps.ConfigurationAsCode.Azure;
using CodeOps.InfrastructureAsCode;
using CodeOps.InfrastructureAsCode.Azure;
using CodeOps.EnvironmentAsCode;
using AppConfig = CodeOps.ConfigurationAsCode.Azure.AzureAppConfigurationProvider;
using KeyVault = CodeOps.ConfigurationAsCode.Azure.AzureKeyVaultProvider;

namespace Examples.WebApp;

internal sealed partial class Environment :
    InfraAsCode.IEntry<ConfigAsCode.Builder>,
    ConfigAsCode.IEntry<Sentinel>
{
    public InfraAsCode.Entry<ConfigAsCode.Builder> InfrastructureAsCode(InfraAsCode.Context<ConfigAsCode.Builder> context)
    {
        return MatchEnvironment(
            prod =>
                Create(
                    new AppConfig.Name("AAC-P"),
                    new AppConfig.Label("App"),
                    AppConfig.Sku.Free,
                    new KeyVault.Name("KV-P"),
                    KeyVault.Sku.Standard),
            test =>
                Create(
                    new AppConfig.Name("AAC-T"),
                    new AppConfig.Label("App"),
                    AppConfig.Sku.Free,
                    new KeyVault.Name("KV-T"),
                    KeyVault.Sku.Standard));

        InfraAsCode.Entry<ConfigAsCode.Builder> Create(AppConfig.Name name, AppConfig.Label label, AppConfig.Sku sku, KeyVault.Name keyVaultName, KeyVault.Sku keyVaultSku)
        {
            return AppConfig
                .Create(name, label, sku, keyVaultName, keyVaultSku, this.GetArgument<ConfigAsCode.Enabled>(), Builder.Services, Builder.Configuration, this)
                .InfraAsCode(GetAzureOptions(), context);
        }
    }

    public ConfigAsCode.Entry<Sentinel> ConfigurationAsCode(ConfigAsCode.Context<Sentinel> context)
    {
        return MatchEnvironment(
            prod => context.Value(1),
            test => context.Value(1));
    }
}