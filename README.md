# CodeOps

[![Release Status](https://img.shields.io/github/actions/workflow/status/HubiBoar/CodeOps/publish.yml)](https://github.com/HubiBoar/CodeOps/actions/workflows/publish.yml)

**CodeOps** Is a framework aiming to move as much DevOps work from yamls etc. into C# as part of your application code.<br>
It is Cloud agnostic and can be extended to support any Cloud Provider, for now it focues on Azure.

**This project and samples are still WORK IN PROGRESS.**


## [CodeOps.EnvironmentAsCode](src/Examples/WebApp/Host/Environment/Environment.cs)

[![NuGet Version](https://img.shields.io/nuget/v/CodeOps.EnvironmentAsCode)](https://www.nuget.org/packages/CodeOps.EnvironmentAsCode/)

**CodeOps.EnvironmentAsCode** Is a library for programmatically expressing environments for your app <br>such as Development, Test, Staging, Production etc.

Recomended approach is to create a partial Environment class in a separate folder [/Environment/Environment.cs](src/Examples/WebApp/Host/Environment/Environment.cs) at the Program.cs level.
This Environment class will be a base for most of CodeOps setup, stored in a different files extending the partial class.

```csharp
public sealed partial class Environment : EnvironmentAsCode<
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
```

## [CodeOps.InfrastructureAsCode](src/Examples/WebApp/Host/Environment/InfraAsCode.DbConnection.cs)

[![NuGet Version](https://img.shields.io/nuget/v/CodeOps.InfrastructureAsCode)](https://www.nuget.org/packages/CodeOps.InfrastructureAsCode/)

**CodeOps.InfrastructureAsCode** Is a library for programmatically designing your Cloud Infrastructure.

Recommended approach would be to extend the partial Environment class with a new file [/Environment/InfraAsCode.DbConnection.cs](src/Examples/WebApp/Host/Environment/InfraAsCode.DbConnection.cs).
<br>This class will be creating an SqlServerConnection with different setups based on current Environment.

```csharp
public sealed partial class Environment :
    InfraAsCode.IEntry<SqlServerConnection>
{
    public InfraAsCode.Entry<SqlServerConnection> InfrastructureAsCode(InfraAsCode.Context<SqlServerConnection> context)
    {
        return MatchEnvironment(
            prod =>
                Create(
                    new Sql.ServerName("SQL-P"),
                    new Sql.DbName("sql-db"),
                    new SqlSku("Standard")),
            test =>
                Create(
                    new Sql.ServerName("SQL-T"),
                    new Sql.DbName("sql-db"),
                    new SqlSku("Basic")));

        InfraAsCode.Entry<SqlServerConnection> Create(Sql.ServerName serverName, Sql.DbName dbName, SqlSku sku)
        {
            return new Sql(serverName, dbName, sku, _ => {})
                .InfraAsCode(this.GetArgument<AzureOptions>(), context);
        }
    }
}
```

## [CodeOps.ConfigurationAsCode](src/Examples/Samples/ConfigAsCodeExample.cs)

[![NuGet Version](https://img.shields.io/nuget/v/CodeOps.ConfigurationAsCode)](https://www.nuget.org/packages/CodeOps.ConfigurationAsCode/)

**CodeOps.ConfigurationAsCode** Is a library for managing configuration inside the application code, <br>with the ability to validate and deploy it to an external source of Configuration such as Azure AppConfiguration.

It supports Values, Sections and FeatureToggles.

```csharp
public sealed partial class Environment :
    ConfigAsCode.IEntry<FeatureToggle<Feature>>,
    ConfigAsCode.IEntry<Section>,
    ConfigAsCode.IEntry<Value>
{
    public ConfigAsCode.Entry<FeatureToggle<Feature>> ConfigurationAsCode(ConfigAsCode.Context<FeatureToggle<Feature>> context)
    {
        return MatchEnvironment(
            prod => 
                context.Filters(
                    new FeatureToggle.Percentage(50),
                    new FeatureToggle.TimeWindow(
                        DateTimeOffset.Now,
                        DateTimeOffset.Now.AddDays(5))),
            test =>
                context.Filters(
                    new FeatureToggle.TimeWindow(
                        DateTimeOffset.Now,
                        DateTimeOffset.Now.AddDays(5))));
    }

    public ConfigAsCode.Entry<Section> ConfigurationAsCode(ConfigAsCode.Context<Section> context)
    {
        return MatchEnvironment(
            prod => 
                context
                    .Value(section => section.Value1, "ProdSectionValue1")
                    .Manual(section => section.Value2)
                    .Reference(section => section.Value3, "ConnectionString"),
            test =>
                context
                    .Value(section => section.Value1, "TestSectionValue1")
                    .Manual(section => section.Value2)
                    .Reference(section => section.Value3, "ConnectionString"));
    }

    public ConfigAsCode.Entry<Value> ConfigurationAsCode(ConfigAsCode.Context<Value> context)
    {
        return MatchEnvironment(
            prod => context.Value("ProdValue"),
            test => context.Manual());
    }
```


## [CodeOps.DeploymentAsCode](src/Examples/WebApp/Host/Environment/InfraAsCode.Deployment.cs)

[![NuGet Version](https://img.shields.io/nuget/v/CodeOps.DeploymentAsCode)](https://www.nuget.org/packages/CodeOps.DeploymentAsCode/)

**CodeOps.DeploymentAsCode** Is a library helping with easier application code deployment into <br>for example Azure Containers Registry as a Docker and then into Azure App Serivice.
