using CodeOps.ConfigurationAsCode;
using Definit.Configuration;
using Definit.Validation;
using Definit.Validation.FluentValidation;
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

    protected override OneOf<Success, ValidationErrors> Validate(Validator<Section> context)
    {
        return context.Fluent(validator =>
        {
            validator.RuleFor(x => x.Value1).IsConnectionString();

            validator.RuleFor(x => x.Value2).IsConnectionString();
        });
    }
}

internal sealed class Value : ConfigValue<Value, string, IsConnectionString>
{
    protected override string SectionName => "Name";
}

internal sealed partial class Environment : 
    ConfigAsCode.ISetup<FeatureToggle<Feature>>,
    ConfigAsCode.ISetup<Section>,
    ConfigAsCode.ISetup<Value>
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
                    .Manual(section => section.Value2),
            acc =>
                context
                    .Value(section => section.Value1, "AccSectionValue1")
                    .Manual(section => section.Value2),
            test =>
                context
                    .Value(section => section.Value1, "TestSectionValue1")
                    .Manual(section => section.Value2));
    }

    public ConfigAsCode.Entry<Value> ConfigurationAsCode(ConfigAsCode.Context<Value> context)
    {
        return MatchEnvironment(
            prod => 
                context.Value("ProdValue"),
            acc =>
                context.Value("Accvalue"),
            test =>
                context.Value("testValue"));
    }
}
