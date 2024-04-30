using CodeOps.ArgumentAsCode;
using CodeOps.ConfigurationAsCode;
using CodeOps.EnvironmentAsCode;
using Definit.Configuration;
using Definit.Validation;
using Definit.Validation.FluentValidation;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OneOf;
using OneOf.Types;

namespace Samples;

internal sealed class Feature : IFeatureName
{
    public static string FeatureName => "Test";
}

internal sealed class Section : ConfigSection<Section>
{
    protected override string SectionName { get; } = "ExampleConfigSection";

    public string Value1 { get; } = string.Empty;
    
    public string Value2 { get; } = string.Empty;

    public string Value3 { get; } = string.Empty;

    protected override ValidationResult Validate(Validator<Section> context)
    {
        return context.Fluent(validator =>
        {
            validator.RuleFor(x => x.Value1).NotEmpty();

            validator.RuleFor(x => x.Value3).EmailAddress();

            validator.RuleFor(x => x.Value3).IsConnectionString();
        });
    }
}

internal sealed class Value : ConfigValue<Value, string, IsConnectionString>
{
    protected override string SectionName => "Name";
}

internal static class HostExample
{
    private static void Run(IHostApplicationBuilder builder)
    {
        FeatureToggle<Feature>.Register(builder.Services, builder.Configuration);
        Section.Register(builder.Services, builder.Configuration);
        Value.Register(builder.Services, builder.Configuration);        
    }
}

internal sealed partial class Environment :
    ConfigAsCode.IEntry<FeatureToggle<Feature>>,
    ConfigAsCode.IEntry<Section>,
    ConfigAsCode.IEntry<Value>,
    ArgAsCode.IEntry<ConfigAsCode.Enabled>
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
            acc =>
                context.Filters(
                    new FeatureToggle.Percentage(50)),
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
            acc =>
                context
                    .Value(section => section.Value1, "AccSectionValue1")
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
            acc  => context.Reference("AccValue"),
            test => context.Manual());
    }

    public ArgAsCode.Entry<ConfigAsCode.Enabled> ArgumentAsCode(ArgAsCode.Context<ConfigAsCode.Enabled> context)
    {
        return MatchEnvironment(
            prod =>
                context
                    .DefaultValue(new ConfigAsCode.Enabled(true))
                    .FromArgs(Args, "cac", "config-as-code", arg => new ConfigAsCode.Enabled(bool.Parse(arg)))
                    .FromConfig(Builder.Configuration, config => new ConfigAsCode.Enabled(config.GetValue<bool>("ConfigAsCode"))),
            acc  =>
                context
                    .DefaultValue(new ConfigAsCode.Enabled(true))
                    .FromArgs<ConfigAsCodeEnabled>(Args)
                    .FromConfig<ConfigAsCodeEnabled>(Builder.Configuration),
            test => new ConfigAsCode.Enabled(true));
    }

    private struct ConfigAsCodeEnabled : ArgAsCode.IArgument<ConfigAsCodeEnabled, ConfigAsCode.Enabled, bool, IsNotNull<bool>>
    {
        public static string SectionName => "ConfigAsCode";

        public static string ArgumentShortcut => "cac";

        public static string ArgumentFullName => "config-as-code";

        public static ConfigAsCode.Enabled Map(bool value) => new (value);
    }
}
