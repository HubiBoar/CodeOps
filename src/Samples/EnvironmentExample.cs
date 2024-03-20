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

    private IHostApplicationBuilder Builder { get; }
    private string[] Args { get; }

    public Environment(IHostApplicationBuilder builder, string[] args) : base(GetSettingFromConfiguration(builder.Configuration))
    {
        Builder = builder;
        Args = args;
    }
}