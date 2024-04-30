using CodeOps.EnvironmentAsCode;

namespace Examples.WebApp;

internal sealed partial class Environment : EnvAsCode.Base<
    Environment.Prod,
    Environment.Test>
{
    public sealed record Prod : EnvAsCode.IVersion
    {
        public static string Name => "Prod";
    }

    public sealed record Test : EnvAsCode.IVersion
    {
        public static string Name => "Test";
    }

    private IHostApplicationBuilder Builder { get; }

    public Environment(IHostApplicationBuilder builder, string[] args) : base(builder.Configuration, args)
    {
        Builder = builder;
    }
}