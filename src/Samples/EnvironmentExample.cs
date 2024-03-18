using Azure.Core;
using CodeOps.ConfigurationAsCode;
using CodeOps.EnvironmentAsCode;
using CodeOps.InfrastructureAsCode.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

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

    private ConfigAsCode.Enabled ConfigAsCodeEnabled { get; }

    private IHostApplicationBuilder Builder { get; }

    private AzureDeploymentLocation AzureDeploymentLocation { get; } = new (AzureLocation.WestEurope, "Test-RG", "Test-Sub");

    public Environment(IConfiguration configuration, ConfigAsCode.Enabled configAsCodeEnabled, IHostApplicationBuilder builder) : base(GetSettingFromConfiguration(configuration))
    {
        ConfigAsCodeEnabled = configAsCodeEnabled;
        Builder = builder;
    }
}