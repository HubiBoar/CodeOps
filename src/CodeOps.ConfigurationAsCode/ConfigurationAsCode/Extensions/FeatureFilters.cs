using System.Text.Json;
using Definit.Configuration;
using Definit.Validation;
using Microsoft.FeatureManagement.FeatureFilters;
using OneOf;
using OneOf.Types;

namespace CodeOps.ConfigurationAsCode;

public static class FeatureToggle
{
    public interface IFilter
    {
        public string Name { get; }

        public object Parameters { get; }
    }

    public interface IFilter<T> : IFilter
    {
        public new T Parameters { get; }
    }

    public sealed class Percentage : IFilter<PercentageFilterSettings>
    {
        public string Name => "Microsoft.Percentage";

        public PercentageFilterSettings Parameters { get; }
        object IFilter.Parameters => Parameters;

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
        object IFilter.Parameters => Parameters;

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
        object IFilter.Parameters => Parameters;

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
    public static ValidationResult AddConfig<TSection>(
        this ConfigAsCode.Builder builder,        
        ConfigAsCode.IEntry<FeatureToggle<TSection>> configAsCode)
        where TSection : IFeatureName
    {
        return builder.AddConfig(FeatureToggle<TSection>.Register, configAsCode);
    }

    public static ConfigAsCode.Entry<FeatureToggle<T>> Filters<T>(
        this ConfigAsCode.Context<FeatureToggle<T>> _,
        params FeatureToggle.IFilter[] filters)
        where T : IFeatureName
    {
        var features = new ConfigAsCode.FiltersEnabledFor(
            filters
                .Select(x => new ConfigAsCode.FeatureFilter(x.Name, x.Parameters))
                .ToArray());

        return new ConfigAsCode.Entry<FeatureToggle<T>>(
            new ConfigAsCode.Path(FeatureToggle<T>.SectionName),
            features,
            FeatureToggle<T>.ValidateConfiguration);
    }

    public static ConfigAsCode.Entry<FeatureToggle<T>> Value<T>(
        this ConfigAsCode.Context<FeatureToggle<T>> _,
        bool value)
        where T : IFeatureName
    {
        return new ConfigAsCode.Entry<FeatureToggle<T>>(
            new ConfigAsCode.Path(FeatureToggle<T>.SectionName),
            new ConfigAsCode.FeatureFlag(value),
            FeatureToggle<T>.ValidateConfiguration);
    }

    public static ConfigAsCode.Entry<FeatureToggle<T>> Manual<T>(
        this ConfigAsCode.Context<FeatureToggle<T>> _)
        where T : IFeatureName
    {
        return new ConfigAsCode.Entry<FeatureToggle<T>>(
            new ConfigAsCode.Path(FeatureToggle<T>.SectionName),
            new ConfigAsCode.Manual(),
            FeatureToggle<T>.ValidateConfiguration);
    }

    public static ConfigAsCode.Entry<FeatureToggle<T>> Reference<T>(
        this ConfigAsCode.Context<FeatureToggle<T>> _,
        string path)
        where T : IFeatureName
    {
        return new ConfigAsCode.Entry<FeatureToggle<T>>(
            new ConfigAsCode.Path(FeatureToggle<T>.SectionName),
            new ConfigAsCode.Reference(new ConfigAsCode.Path(path)),
            FeatureToggle<T>.ValidateConfiguration);
    }
}
