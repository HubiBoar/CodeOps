using Azure.Core;
using CodeOps.InfrastructureAsCode.Azure;

namespace Examples.WebApp;

internal sealed partial class Environment
{
    private AzureOptions GetAzureOptions()
    {
        return MatchEnvironment(
            prod => new AzureOptions(AzureLocation.WestEurope, "RG-P", "Prod-Sub"), 
            test => new AzureOptions(AzureLocation.WestEurope, "RG-T", "Test-Sub"));
    }
}