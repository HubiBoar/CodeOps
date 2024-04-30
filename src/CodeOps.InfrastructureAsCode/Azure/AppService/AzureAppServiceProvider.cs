using Azure;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using CodeOps.EnvironmentAsCode;
using CodeOps.DeploymentAsCode;
using CodeOps.ArgumentAsCode;

namespace CodeOps.InfrastructureAsCode.Azure;

internal sealed record AppSerivceDeployCode : IDeployCode
{
    private readonly Func<Task> _onDeploy;

    public AppSerivceDeployCode(Func<Task> onDeploy)
    {
        _onDeploy = onDeploy;
    }

    public Task DeployCode()
    {
        return _onDeploy();
    }
}

public static class AzureAppServiceProvider
{
    public sealed record PlanName(string Value);
    public sealed record AppName(string Value);

    public static async Task<IDeployCode> Provision(
        AzureDeploymentOptions options,
        PlanName planName,
        AppName appName,
        AppServiceSkuDescription sku,
        Func<SiteConfigProperties, Task> deployCode,
        Func<SiteConfigProperties, Task> modifyProperties)
    {
        var location = options.Location;
        var resourceGroup = options.ResourceGroup;

        var appServicePlanData = new AppServicePlanData(location)
        {
            Sku = sku
        };

        var appServicePlanResult = await resourceGroup
            .GetAppServicePlans()
            .CreateOrUpdateAsync(
                WaitUntil.Completed,
                planName.Value,
                appServicePlanData);

        var appServiceData = new WebSiteData(location)
        {
            AppServicePlanId = appServicePlanResult.Value.Data.Id,
        };

        await modifyProperties(appServiceData.SiteConfig);

        await resourceGroup
            .GetWebSites()
            .CreateOrUpdateAsync(
                WaitUntil.Completed,
                appName.Value,
                appServiceData);

        return GetInternal(options, appName, deployCode);
    }

    public static Task<IDeployCode> Get(AzureDeploymentOptions options, AppName appName, Func<SiteConfigProperties, Task> deployCode)
    {
        return GetInternal(options, appName, deployCode).AsTask();
    }

    private static IDeployCode GetInternal(AzureDeploymentOptions options, AppName appName, Func<SiteConfigProperties, Task> deployCode)
    {
        return new AppSerivceDeployCode(() => DeployCode(options, appName, deployCode));
    }

    private static async Task DeployCode(AzureDeploymentOptions options, AppName appName, Func<SiteConfigProperties, Task> deployCode)
    {
        var resourceGroup = options.ResourceGroup;

        var webSite = await resourceGroup
            .GetWebSiteAsync(appName.Value);

        var siteConfig = webSite.Value.Data.SiteConfig;

        await deployCode(siteConfig);

        var update = new SitePatchInfo()
        {
            SiteConfig = siteConfig
        };

        await webSite.Value.UpdateAsync(update);
    }
}