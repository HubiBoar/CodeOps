using Azure.ResourceManager.AppService.Models;
using AppService = CodeOps.InfrastructureAsCode.Azure.AzureAppServiceProvider;
using CodeOps.DeploymentAsCode;

namespace CodeOps.InfrastructureAsCode.Azure;

public static class AzureAppServiceProviderExtensions
{
    public static InfraAsCode.Entry<IDeployCode> AddAzureAppService(
        this InfraAsCode.Context<IDeployCode> context,
        AzureOptions location,
        AppService.PlanName planName,
        AppService.AppName appName,
        AppServiceSkuDescription sku,
        string containerName,
        InfraAsCode.IEntry<ContainerHub> containerHubEntry)
    {            
        return context.AzureInfraAsCode(
            location,
            async (get, options) => 
            {
                var containerHub = await get.Get(containerHubEntry);

                return await AppService.Get(
                    options,
                    appName,
                    ModifySiteConfig(containerHub, containerName));
            },
            async (provision, options) => 
            {
                var containerHub = await provision.Provision(containerHubEntry);

                return await AppService.Provision(
                    options,
                    planName,
                    appName,
                    sku,
                    ModifySiteConfig(containerHub, containerName),
                    ModifySiteConfig(containerHub, containerName));
            });
    }

    private static Func<SiteConfigProperties, Task> ModifySiteConfig(ContainerHub hub, string containerName)
    {
        return async siteConfig => 
        {
            var (_, tag) = await hub.DeployCode(containerName);
            var dockerRegistryName = hub.Name;
            var dockerRegistryPath = hub.Uri;
            var dockerPath = $"{dockerRegistryPath}/{containerName}:{tag}";

            siteConfig.LinuxFxVersion = $"DOCKER|{dockerPath}";

            siteConfig.AppSettings.Add(new AppServiceNameValuePair()
            {
                Name = "DOCKER_CUSTOM_IMAGE_NAME",
                Value = dockerPath
            });

            siteConfig.AppSettings.Add(new AppServiceNameValuePair()
            {
                Name = "DOCKER_REGISTRY_SERVER_URL",
                Value = dockerRegistryName
            });
        };
    }

}