using CodeOps.ArgumentAsCode;
using CodeOps.InfrastructureAsCode;
using CodeOps.InfrastructureAsCode.Azure;
using CodeOps.EnvironmentAsCode;
using Azure.ResourceManager.ContainerRegistry.Models;
using Azure.ResourceManager.AppService.Models;
using AppService = CodeOps.InfrastructureAsCode.Azure.AzureAppServiceProvider;
using Container = CodeOps.InfrastructureAsCode.Azure.AzureContainerRegistryProvider;
using CodeOps.DeploymentAsCode;

namespace Examples.WebProject;

internal sealed partial class Environment :
    InfraAsCode.IEntry<IDeployCode>,
    InfraAsCode.IEntry<ContainerHub>
{
    public InfraAsCode.Entry<IDeployCode> InfrastructureAsCode(InfraAsCode.Context<IDeployCode> context)
    {
        return MatchEnvironment(
            prod =>
                Create(
                    new AppService.PlanName("AppPlan-P"),
                    new AppService.AppName("App-P"),
                    new AppServiceSkuDescription()
                    {
                        Name = "B1"
                    }),
            test =>
                Create(
                    new AppService.PlanName("AppPlan-T"),
                    new AppService.AppName("App-T"),
                    new AppServiceSkuDescription() 
                    {
                        Name = "B1"
                    }));

        InfraAsCode.Entry<IDeployCode> Create(
            AppService.PlanName planName,
            AppService.AppName appName,
            AppServiceSkuDescription sku)
        {
            return context.AddAzureAppService(this.GetArgument<AzureOptions>(), planName, appName, sku, "app", this);
        }
    }

    public InfraAsCode.Entry<ContainerHub> InfrastructureAsCode(InfraAsCode.Context<ContainerHub> context)
    {
        return MatchEnvironment(
            prod =>
                Create(
                    new Container.Name("CR-P"),
                    ContainerRegistrySkuName.Basic),
            test =>
                Create(
                    new Container.Name("CR-T"),
                    ContainerRegistrySkuName.Basic));

        InfraAsCode.Entry<ContainerHub> Create(
            Container.Name name,
            ContainerRegistrySkuName sku)
        {
            return new Container(name, sku)
                .InfraAsCode(this.GetArgument<AzureOptions>(), context);
        }
    }
}