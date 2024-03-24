using Azure;
using Azure.ResourceManager.ContainerRegistry;
using Azure.ResourceManager.ContainerRegistry.Models;
using CodeOps.EnvironmentAsCode;
using CodeOps.DeploymentAsCode;

namespace CodeOps.InfrastructureAsCode.Azure;

public sealed class AzureContainerRegistryProvider : IAzureComponentProvider<ContainerHub>
{
    public sealed record Name(string Value);

    private readonly Name _name;
    private readonly ContainerRegistrySkuName _sku;

    public AzureContainerRegistryProvider(Name name, ContainerRegistrySkuName sku)
    {
        _name = name;
        _sku = sku;
    }

    public Task<ContainerHub> Provision(AzureDeploymentOptions options)
    {
        var location = options.Location;
        var resourceGroup = options.ResourceGroup;

        var data = new ContainerRegistryData(location, new ContainerRegistrySku(_sku));

        resourceGroup
            .GetContainerRegistries()
            .CreateOrUpdate(
                WaitUntil.Completed,
                _name.Value,
                data);

        return Get(options);
    }

    public Task<ContainerHub> Get(AzureDeploymentOptions options)
    {
        var uri = GetUri(_name);
        return new ContainerHub(_name.Value, uri, $"login {uri}").AsTask();
    }

    private static Uri GetUri(Name name)
    {
        return new Uri($"{name.Value}.azurecr.io");
    }
}