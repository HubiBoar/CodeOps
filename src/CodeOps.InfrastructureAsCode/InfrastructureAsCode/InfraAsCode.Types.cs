namespace CodeOps.InfrastructureAsCode;

public sealed partial class InfraAsCode
{
    public interface IComponent
    {
    }

    public interface ISetup<T>
        where T : IComponent
    {
        public Entry<T> InfrastructureAsCode(Context<T> context);
    }

    public sealed class Context<T>
        where T : IComponent
    {
        internal Context()
        {
        }
    }

    public sealed record Entry<T>(Func<Task<T>> Factory)
        where T : IComponent;
}

public static class InfraAsCodeExtensions
{
    public static Task<T> Provision<T>(InfraAsCode.ISetup<T> infraAsCode)
        where T : InfraAsCode.IComponent
    {
        var context = new InfraAsCode.Context<T>();
        var entry = infraAsCode.InfrastructureAsCode(context);

        return entry.Factory();
    }
}