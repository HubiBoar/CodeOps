using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Definit.Utils;

namespace CodeOps.InfrastructureAsCode.Azure;

public sealed class AzureDeploymentOptions
{
    public SubscriptionResource Subscription { get; }
    public AzureLocation Location { get; }
    public ResourceGroupResource ResourceGroup { get; }
    public TokenCredential Credentials { get; }
    
    private AzureDeploymentOptions(
        SubscriptionResource subscription,
        AzureLocation location,
        ResourceGroupResource resourceGroup,
        TokenCredential credentials)
    {
        Subscription = subscription;
        Location = location;
        ResourceGroup = resourceGroup;
        Credentials = credentials;
    }
    
    public static AzureDeploymentOptions Create(AzureOptions deploymentLocation)
    {
        var location = deploymentLocation.Location;
        var resourceGroupName = deploymentLocation.ResourceGroupName;
        var credentials = deploymentLocation.Credential;

        var subscription = new ArmClient(credentials).GetSubscriptions()
            .Single(x => x.Data.DisplayName == deploymentLocation.SubscriptionName);
        
        var resourceGroupData = new ResourceGroupData(location);

        var resources = subscription.GetResourceGroups();
        if (resources.Exists(resourceGroupName))
        {
            var resourceGroup = resources.Get(resourceGroupName);

            return new AzureDeploymentOptions(subscription, location, resourceGroup, credentials);
        }
        else
        {
            var resourceGroup = resources.CreateOrUpdate(
                WaitUntil.Completed,
                resourceGroupName,
                resourceGroupData).Value;

            return new AzureDeploymentOptions(subscription, location, resourceGroup, credentials);
        }
    }
}

public sealed record AzureOptions(AzureLocation Location, string ResourceGroupName, string SubscriptionName, TokenCredential Credential)
{
    public AzureOptions(AzureLocation location, string resourceGroupName, string subscriptionName) : this(location, resourceGroupName, subscriptionName, DefaultCredentials())
    {
    }

    public static TokenCredential DefaultCredentials()
    {
        try
        {
            return new DefaultAzureCredential();
        }
        catch
        {
            throw new ForgotToAzLogin("DefaultAzureCredential Exception. Try using command 'az login' in the terminal");
        }
    }
}

public interface IAzureComponentProvider<TComponent>
    where TComponent : InfraAsCode.IComponent
{
    protected Task<TComponent> Get(
        AzureDeploymentOptions options);
    
    protected Task<TComponent> Provision(
        AzureDeploymentOptions options);

    public virtual InfraAsCode.Entry<TComponent> InfraAsCode(
        AzureOptions location,
        InfraAsCode.Context<TComponent> context)
    {
        return context.AzureInfraAsCode(location, Get, Provision);
    }
}

public static class AzureComponentProviderExtensions
{
    public static InfraAsCode.Entry<TComponent> InfraAsCode<TComponent>(this IAzureComponentProvider<TComponent> provider, AzureOptions location, InfraAsCode.Context<TComponent> context)
        where TComponent : InfraAsCode.IComponent
    {
        return provider.InfraAsCode(location, context);
    }

     public static InfraAsCode.Entry<TComponent> AzureInfraAsCode<TComponent>(
        this InfraAsCode.Context<TComponent> context,
        AzureOptions location,
        Func<AzureDeploymentOptions, Task<TComponent>> getFactory,
        Func<AzureDeploymentOptions, Task<TComponent>> provisionFactory)
        where TComponent : InfraAsCode.IComponent
    {
        return context.AzureInfraAsCode(location, (_, options) => getFactory(options), (_, options) => provisionFactory(options));
    }

    public static InfraAsCode.Entry<TComponent> AzureInfraAsCode<TComponent>(
        this InfraAsCode.Context<TComponent> _,
        AzureOptions location,
        Func<InfraAsCode.Get, AzureDeploymentOptions, Task<TComponent>> getFactory,
        Func<InfraAsCode.Provision, AzureDeploymentOptions, Task<TComponent>> provisionFactory)
        where TComponent : InfraAsCode.IComponent
    {
        var type = typeof(TComponent);
        return new InfraAsCode.Entry<TComponent>(
            get =>
            {
                var options = AzureDeploymentOptions.Create(location);
                Console.WriteLine($"--- Get: {type.GetTypeVerboseName()}");

                var result = getFactory(get, options);

                Console.WriteLine($"--- Provisioning Finished");

                return result;
            },
            provision =>
            {
                var options = AzureDeploymentOptions.Create(location);
                Console.WriteLine($"--- Provisioning: {type.GetTypeVerboseName()}");
                
                var result = provisionFactory(provision, options);
                
                Console.WriteLine($"--- Provisioning Finished");

                return result;
            });
    }
}