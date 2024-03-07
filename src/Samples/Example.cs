using CodeOps.ConfigurationAsCode;
using Definit.Configuration;
using Definit.Validation;
using Definit.Validation.FluentValidation;
using OneOf;
using OneOf.Types;

namespace Samples;

file static class Example
{
    private sealed class Feature : IFeatureName
    {
        public static string FeatureName => "Test";
    }

    private sealed class Section : ConfigSection<Section>
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

    private sealed class Value : ConfigValue<Value, string, IsConnectionString>
    {
        protected override string SectionName => "Name";
    }

    private sealed class ExampleConfigAsCodeSetup : 
        ConfigAsCode.ISetup<FeatureToggle<Feature>>,
        ConfigAsCode.ISetup<Section>,
        ConfigAsCode.ISetup<Value>
    {
        public ConfigAsCode.Entry<FeatureToggle<Feature>> ConfigurationAsCode(ConfigAsCode.Context<FeatureToggle<Feature>> context)
        {
            return context.Filters(
                new FeatureToggle.Percentage(50),
                new FeatureToggle.TimeWindow(
                    DateTimeOffset.Now,
                    DateTimeOffset.Now.AddDays(5)));
        }

        public ConfigAsCode.Entry<Section> ConfigurationAsCode(ConfigAsCode.Context<Section> context)
        {
            return context
                .Value(section => section.Value1, "testSectionValue1")
                .Manual(section => section.Value2);
        }

        public ConfigAsCode.Entry<Value> ConfigurationAsCode(ConfigAsCode.Context<Value> context)
        {
            return context.Value("testValue");
        }
    }
}
