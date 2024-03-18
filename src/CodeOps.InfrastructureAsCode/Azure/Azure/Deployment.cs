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
    
    private AzureDeploymentOptions(
        SubscriptionResource subscription,
        AzureLocation location,
        ResourceGroupResource resourceGroup)
    {
        Subscription = subscription;
        Location = location;
        ResourceGroup = resourceGroup;
    }
    
    public static AzureDeploymentOptions Create(AzureDeploymentLocation deploymentLocation, AzureCredentails credentials)
    {
        var location = deploymentLocation.Location;
        var resourceGroupName = deploymentLocation.ResourceGroupName;

        var subscription = new ArmClient(credentials.Credentials).GetSubscriptions()
            .Single(x => x.Data.DisplayName == deploymentLocation.SubscriptionName);
        
        var resourceGroupData = new ResourceGroupData(location);

        var resources = subscription.GetResourceGroups();
        if (resources.Exists(resourceGroupName))
        {
            var resourceGroup = resources.Get(resourceGroupName);

            return new AzureDeploymentOptions(subscription, location, resourceGroup);
        }
        else
        {
            var resourceGroup = resources.CreateOrUpdate(
                WaitUntil.Completed,
                resourceGroupName,
                resourceGroupData).Value;

            return new AzureDeploymentOptions(subscription, location, resourceGroup);
        }
    }
}

public sealed record AzureDeploymentLocation(AzureLocation Location, string ResourceGroupName, string SubscriptionName);

public sealed class AzureCredentails
{
    public TokenCredential Credentials { get; }

    internal AzureCredentails()
    {
        try
        {
            Credentials = new DefaultAzureCredential();
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
    protected TComponent Get(
        AzureCredentails credentials);
    
    protected TComponent Provision(
        AzureCredentails credentials,
        AzureDeploymentOptions options);

    public virtual InfraAsCode.Entry<TComponent> InfraAsCode(
        AzureDeploymentLocation location,
        InfraAsCode.Context<TComponent> context)
    {
        return new InfraAsCode.Entry<TComponent>(() => 
        {
            var credentials = new AzureCredentails();

            return context.Match(
                get => Get(credentials),
                provision =>
                {
                    var settings = AzureDeploymentOptions.Create(location, credentials);
                    Console.WriteLine($"--- Provisioning: {GetType().GetTypeVerboseName()}");
                    
                    var result = Provision(credentials, settings);
                    
                    Console.WriteLine($"--- Provisioning Finished");

                    return result;
                });
        });
    }
}

public static class AzureComponentProviderExtensions
{
    public static InfraAsCode.Entry<TComponent> InfraAsCode<TComponent>(this IAzureComponentProvider<TComponent> provider, AzureDeploymentLocation location, InfraAsCode.Context<TComponent> context)
        where TComponent : InfraAsCode.IComponent
    {
        return provider.InfraAsCode(location, context);
    }
}