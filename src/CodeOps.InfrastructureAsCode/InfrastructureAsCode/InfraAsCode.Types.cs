using OneOf;
using OneOf.Else;

namespace CodeOps.InfrastructureAsCode;

public sealed partial class InfraAsCode
{
    public sealed record Enabled(bool IsEnabled);

    public interface IEntry<T>
        where T : IComponent
    {
        public Entry<T> InfrastructureAsCode(Context<T> context);
    }

    public interface IComponent
    {
    }

    public sealed class Context<T> : OneOfBase<Context<T>.Get, Context<T>.Provision>
        where T : IComponent
    {
        public sealed record Get();
        public sealed record Provision();

        internal Context(Enabled getOrProvision) :
            base(
                getOrProvision
                    .IsEnabled
                    .Match<OneOf<Get, Provision>>(
                        () => new Provision(),
                        () => new Get()))
        {
        }
    }

    public sealed record Entry<T>(Func<T> Factory)
        where T : IComponent;
}

public static class InfraAsCodeHelper
{
    public static T CreateComponent<T>(
        InfraAsCode.IEntry<T> infraAsCode,
        InfraAsCode.Enabled getOrProvision)
        where T : InfraAsCode.IComponent
    {
        var context = new InfraAsCode.Context<T>(getOrProvision);
        var entry = infraAsCode.InfrastructureAsCode(context);

        return entry.Factory();
    }
}