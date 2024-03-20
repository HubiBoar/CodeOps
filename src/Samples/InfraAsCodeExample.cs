using Azure.Core;
using CodeOps.ConfigurationAsCode;
using CodeOps.ConfigurationAsCode.Azure;
using CodeOps.InfrastructureAsCode;
using CodeOps.InfrastructureAsCode.Azure;
using CodeOps.EnvironmentAsCode;
using AppConfig = CodeOps.ConfigurationAsCode.Azure.AzureAppConfigurationProvider;
using KeyVault = CodeOps.ConfigurationAsCode.Azure.AzureKeyVaultProvider;

namespace Samples;

internal sealed partial class Environment :
    InfraAsCode.IEntry<ConfigAsCode.Builder>,
    ConfigAsCode.IEntry<Sentinel>,
    ArgumentAsCode.IEntry<AzureDeploymentLocation>
{
    public InfraAsCode.Entry<ConfigAsCode.Builder> InfrastructureAsCode(InfraAsCode.Context<ConfigAsCode.Builder> context)
    {
        return MatchEnvironment(
            prod =>
                CreateNullLabel(
                    new AppConfig.Name("AAC-P"),
                    AppConfig.Sku.Free,
                    new KeyVault.Name("KV-P"),
                    KeyVault.Sku.Standard),
            acc =>
                CreateNullLabel(
                    new AppConfig.Name("AAC-A"),
                    AppConfig.Sku.Free,
                    new KeyVault.Name("KV-A"),
                    KeyVault.Sku.Standard),
            test =>
                Create(
                    new AppConfig.Name("AAC-T"),
                    new AppConfig.Label("Test"),
                    AppConfig.Sku.Free,
                    new KeyVault.Name("KV-T"),
                    KeyVault.Sku.Standard));

        InfraAsCode.Entry<ConfigAsCode.Builder> Create(AppConfig.Name name, AppConfig.Label label, AppConfig.Sku sku, KeyVault.Name keyVaultName, KeyVault.Sku keyVaultSku)
        {
            return AppConfig
                .Create(name, label, sku, keyVaultName, keyVaultSku, this.GetArgument<ConfigAsCode.Enabled>(), Builder.Services, Builder.Configuration, this)
                .InfraAsCode(this.GetArgument<AzureDeploymentLocation>(), context);
        }

        InfraAsCode.Entry<ConfigAsCode.Builder> CreateNullLabel(AppConfig.Name name, AppConfig.Sku sku, KeyVault.Name keyVaultName, KeyVault.Sku keyVaultSku)
        {
            return Create(name, new AppConfig.Label(null), sku, keyVaultName, keyVaultSku);
        }
    }

    public ConfigAsCode.Entry<Sentinel> ConfigurationAsCode(ConfigAsCode.Context<Sentinel> context)
    {
        return MatchEnvironment(
            prod => context.Value(1),
            acc  => context.Value(1),
            test => context.Value(1));
    }

    public ArgumentAsCode.Entry<AzureDeploymentLocation> ArgumentAsCode(ArgumentAsCode.Context<AzureDeploymentLocation> context)
    {
        return MatchEnvironment(
            prod => new AzureDeploymentLocation(AzureLocation.WestEurope, "Prod-RG", "Prod-Sub"), 
            acc  => new AzureDeploymentLocation(AzureLocation.WestEurope, "Acc-RG", "Acc-Sub"),
            test => new AzureDeploymentLocation(AzureLocation.WestEurope, "Test-RG", "Test-Sub"));
    }
}