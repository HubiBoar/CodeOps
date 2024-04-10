using CodeOps.EnvironmentAsCode;

namespace Examples.WebProject;

internal sealed partial class Environment : EnvironmentAsCode<
    Environment.Prod,
    Environment.Test>
{
    public sealed record Prod : IEnvironmentVersion
    {
        public static string Name => "Prod";
    }

    public sealed record Test : IEnvironmentVersion
    {
        public static string Name => "Test";
    }

    private IHostApplicationBuilder Builder { get; }
    private string[] Args { get; }

    public Environment(IHostApplicationBuilder builder, string[] args) : base(GetSettingFromConfiguration(builder.Configuration))
    {
        Builder = builder;
        Args = args;
    }
}