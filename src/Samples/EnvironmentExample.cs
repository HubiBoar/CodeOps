using CodeOps.EnvironmentAsCode;
using Microsoft.Extensions.Configuration;

namespace Samples;

internal sealed partial class Environment : EnvironmentAsCode<
    Environment.Prod,
    Environment.Acc,
    Environment.Test>
{
    public sealed record Prod : IEnvironmentVersion
    {
        public static string Name => "Prod";
    }

    public sealed record Acc : IEnvironmentVersion
    {
        public static string Name => "Acc";
    }

    public sealed record Test : IEnvironmentVersion
    {
        public static string Name => "Test";
    }

    public Environment(IConfiguration configuration) : base(GetSettingFromConfiguration(configuration))
    {
    }
}