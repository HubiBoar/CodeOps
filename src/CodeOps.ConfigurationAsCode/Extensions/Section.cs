using System.Linq.Expressions;
using System.Text.Json;
using Definit.Configuration;

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