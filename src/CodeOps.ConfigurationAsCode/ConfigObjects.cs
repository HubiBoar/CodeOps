using System.Linq.Expressions;
using System.Text.Json;
using Definit.Configuration;
using Definit.Validation;
using Microsoft.Extensions.Configuration;
using OneOf;
using OneOf.Types;

namespace CodeOps.ConfigurationAsCode;

public static class SectionReferenceExtensions
{
    public static ConfigAsCode.Entry<TSection> Value<TSection, TValue>(
        this ConfigAsCode.Context<TSection> _,
        Expression<Func<TSection, TValue>> expression,
        TValue value)
        where TSection : ConfigSection<TSection>, ISectionName, IConfigObject<TSection>, new()
    {
        var sectionName = SectionName(TSection.SectionName, expression);

        var json = JsonSerializer.Serialize(value);

        return new ConfigAsCode.Entry<TSection>(
            sectionName,
            new ConfigAsCode.Value(json),
            TSection.ValidateConfiguration);
    }

    public static ConfigAsCode.Entry<TSection> Manual<TSection, TValue>(
        this ConfigAsCode.Context<TSection> _,
        Expression<Func<TSection, TValue>> expression)
        where TSection : ConfigSection<TSection>, ISectionName, IConfigObject<TSection>, new()
    {
        var sectionName = SectionName(TSection.SectionName, expression);

        return new ConfigAsCode.Entry<TSection>(
            sectionName,
            new ConfigAsCode.Manual(),
            TSection.ValidateConfiguration);
    }

    public static ConfigAsCode.Entry<TSection> Value<TSection, TValue>(
        this ConfigAsCode.Entry<TSection> entry,
        Expression<Func<TSection, TValue>> expression,
        TValue value)
        where TSection : ConfigSection<TSection>, ISectionName, new()
    {
        var sectionName = SectionName(TSection.SectionName, expression);

        var json = JsonSerializer.Serialize(value);

        entry.Entries.Add(sectionName, new ConfigAsCode.Value(json));

        return entry;
    }

    public static ConfigAsCode.Entry<TSection> Manual<TSection, TValue>(
        this ConfigAsCode.Entry<TSection> entry,
        Expression<Func<TSection, TValue>> expression)
        where TSection : ConfigSection<TSection>, ISectionName, new()
    {
        var sectionName = SectionName(TSection.SectionName, expression);

        entry.Entries.Add(sectionName, new ConfigAsCode.Manual());

        return entry;
    }

    private static string SectionName<T>(string sectionName, Expression<T> expression)
    {
        var member = expression.Body as MemberExpression;
        var parameterName = member!.Member.Name;

        return $"{sectionName}:{parameterName}";
    }
}

public static class FeatureExtensions
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


public static class ValueReferenceExtensions
{
    public static ConfigAsCode.Entry<TSection> Value<TSection, TValue>(
        this ConfigAsCode.Context<TSection> _,
        TValue value)
        where TSection : IConfigValue<TValue>, new()
    {
        var json = JsonSerializer.Serialize(value);

        return new ConfigAsCode.Entry<TSection>(
            TSection.SectionName,
            new ConfigAsCode.Value(json),
            TSection.ValidateConfiguration);
    }

    public static ConfigAsCode.Entry<TSection> Manual<TSection, TValue>(
        this ConfigAsCode.Context<TSection> _)
        where TSection : IConfigValue<TValue>, new()
    {
        return new ConfigAsCode.Entry<TSection>(
            TSection.SectionName,
            new ConfigAsCode.Manual(),
            TSection.ValidateConfiguration);
    }
}
