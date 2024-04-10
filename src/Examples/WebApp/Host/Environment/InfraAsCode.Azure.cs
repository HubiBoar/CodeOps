using Azure.Core;
using CodeOps.ArgumentAsCode;
using CodeOps.InfrastructureAsCode.Azure;

namespace Examples.WebProject;

internal sealed partial class Environment :
    ArgumentAsCode.IEntry<AzureOptions>
{
    public ArgumentAsCode.Entry<AzureOptions> ArgumentAsCode(ArgumentAsCode.Context<AzureOptions> context)
    {
        return MatchEnvironment(
            prod => new AzureOptions(AzureLocation.WestEurope, "Prod-RG", "Prod-Sub"), 
            test => new AzureOptions(AzureLocation.WestEurope, "Test-RG", "Test-Sub"));
    }
}