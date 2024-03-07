using System.Text.Json;
using Definit.Configuration;
using Microsoft.FeatureManagement.FeatureFilters;
using Definit.Validation;
using Microsoft.Extensions.Configuration;
using OneOf;
using OneOf.Types;

namespace CodeOps.ConfigurationAsCode;

public static class FeatureToggle
{
    public interface IFilter
    {
        public string Name { get; }
    }

    public interface IFilter<T> : IFilter
    {
        public T Parameters { get; }
    }

    public sealed class Percentage : IFilter<PercentageFilterSettings>
    {
        public string Name => "Microsoft.Percentage";

        public PercentageFilterSettings Parameters { get; }

        public Percentage(int value)
        {
            Parameters = new PercentageFilterSettings()
            {
                Value = value
            };
        }
    }

    public sealed class TimeWindow : IFilter<TimeWindowFilterSettings>
    {
        public string Name => "Microsoft.TimeWindow";

        public TimeWindowFilterSettings Parameters { get; }

        public TimeWindow(DateTimeOffset? start, DateTimeOffset? end)
        {
            Parameters = new TimeWindowFilterSettings()
            {
                Start = start,
                End = end
            };
        }
    }

    public sealed class Targeting : IFilter<TargetingFilterSettings>
    {
        public string Name => "Microsoft.Targeting";

        public TargetingFilterSettings Parameters { get; }

        public Targeting(Audience audience)
        {
            Parameters = new TargetingFilterSettings()
            {
                Audience = audience
            };
        }
    }
}

public static class FeatureToggleExtensions
{
    private sealed record FiltersEnabledFor(IReadOnlyCollection<FeatureToggle.IFilter> EnabledFor);

    public static ConfigAsCode.Entry<FeatureToggle<T>> Filters<T>(
        this ConfigAsCode.Context<FeatureToggle<T>> _,
        params FeatureToggle.IFilter[] filters)
        where T : IFeatureName
    {
        var enabledFor = new FiltersEnabledFor(filters);
        var json = JsonSerializer.Serialize(enabledFor);

        return new ConfigAsCode.Entry<FeatureToggle<T>>(
            FeatureToggle<T>.SectionName,
            new ConfigAsCode.Value(json),
            ValidateConfiguration<T>);
    }

    public static ConfigAsCode.Entry<FeatureToggle<T>> Value<T>(
        this ConfigAsCode.Context<FeatureToggle<T>> _,
        bool value)
        where T : IFeatureName
    {
        return new ConfigAsCode.Entry<FeatureToggle<T>>(
            FeatureToggle<T>.SectionName,
            new ConfigAsCode.Value(value.ToString()),
            ValidateConfiguration<T>);
    }

    private static OneOf<Success, ValidationErrors> ValidateConfiguration<T>(IConfiguration configuration)
        where T : IFeatureName
    {
        var featureName = FeatureToggle<T>.SectionName;
        var section = configuration.GetSection(featureName);

        if(section is null)
        {
            return new ValidationErrors($"Missing FeatureToggle :: [{featureName}]");
        }

        return new Success();
    }
}
