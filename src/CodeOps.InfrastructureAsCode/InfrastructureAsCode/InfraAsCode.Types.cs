using OneOf;
using OneOf.Else;

namespace CodeOps.InfrastructureAsCode;

public sealed partial class InfraAsCode
{
    public sealed record Enabled(bool IsEnabled);
    public sealed record Get();
    public sealed record Provision();

    public interface IEntry<T>
        where T : IComponent
    {
        public Entry<T> InfrastructureAsCode(Context<T> context);
    }

    public interface IComponent
    {
    }

    public sealed record Context<T>()
        where T : IComponent;

    public sealed record Entry<T>(Func<Get, Task<T>> Get, Func<Provision, Task<T>> Provision)
        where T : IComponent
    {
        public Task<T> CreateComponent(Enabled enabled)
        {
            return enabled.IsEnabled.Match(
                () => Get(new Get()),
                () => Provision(new Provision()));
        }

        public Task<T> CreateComponent(OneOf<Get, Provision> oneOf)
        {
            return oneOf.Match(Get, Provision);
        }
    }
}

public static class InfraAsCodeHelper
{
    public static Task<T> CreateComponent<T>(
        InfraAsCode.IEntry<T> infraAsCode,
        InfraAsCode.Enabled getOrProvision)
        where T : InfraAsCode.IComponent
    {
        var context = new InfraAsCode.Context<T>();
        var entry = infraAsCode.InfrastructureAsCode(context);

        return entry.CreateComponent(getOrProvision);
    }

    public static Task<T> Get<T>(
        this InfraAsCode.Get get,
        InfraAsCode.IEntry<T> infraAsCode)
        where T : InfraAsCode.IComponent
    {
        var context = new InfraAsCode.Context<T>();
        var entry = infraAsCode.InfrastructureAsCode(context);

        return entry.CreateComponent(get);
    }

    public static Task<T> Provision<T>(
        this InfraAsCode.Provision provision,
        InfraAsCode.IEntry<T> infraAsCode)
        where T : InfraAsCode.IComponent
    {
        var context = new InfraAsCode.Context<T>();
        var entry = infraAsCode.InfrastructureAsCode(context);

        return entry.CreateComponent(provision);
    }
}