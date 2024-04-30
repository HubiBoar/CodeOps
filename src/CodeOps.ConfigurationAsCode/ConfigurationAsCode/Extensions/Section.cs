using System.Linq.Expressions;
using System.Text.Json;
using Definit.Configuration;
using Definit.Validation;
using OneOf;
using OneOf.Types;

namespace CodeOps.ConfigurationAsCode;

public static class SectionExtensions
{
    public static ValidationResult AddConfig<TSection>(
        this ConfigAsCode.Builder builder,        
        ConfigAsCode.IEntry<TSection> configAsCode)
        where TSection : ConfigSection<TSection>, ISectionName, IConfigObject<TSection>, new()
    {
        return builder.AddConfig(TSection.Register, configAsCode);
    }

    //Full section
    public static ConfigAsCode.Entry<TSection> Value<TSection>(
        this ConfigAsCode.Context<TSection> _,        
        TSection section)
        where TSection : ConfigSection<TSection>, ISectionName, IConfigObject<TSection>, new()
    {
        var json = JsonSerializer.Serialize(section);

        return new ConfigAsCode.Entry<TSection>(
            new ConfigAsCode.Path(TSection.SectionName),
            new ConfigAsCode.Value(json),
            TSection.ValidateConfiguration);
    }

    public static ConfigAsCode.Entry<TSection> Manual<TSection>(
        this ConfigAsCode.Context<TSection> _)
        where TSection : ConfigSection<TSection>, ISectionName, IConfigObject<TSection>, new()
    {
        return new ConfigAsCode.Entry<TSection>(
            new ConfigAsCode.Path(TSection.SectionName),
            new ConfigAsCode.Manual(),
            TSection.ValidateConfiguration);
    }

    public static ConfigAsCode.Entry<TSection> Reference<TSection>(
        this ConfigAsCode.Context<TSection> _,        
        string path)
        where TSection : ConfigSection<TSection>, ISectionName, IConfigObject<TSection>, new()
    {
        return new ConfigAsCode.Entry<TSection>(
            new ConfigAsCode.Path(TSection.SectionName),
            new ConfigAsCode.Reference(new ConfigAsCode.Path(path)),
            TSection.ValidateConfiguration);
    }

    //Per value
    public static ConfigAsCode.Entry<TSection> Value<TSection, TValue>(
        this ConfigAsCode.Context<TSection> _,
        Expression<Func<TSection, TValue>> expression,
        TValue value)
        where TSection : ConfigSection<TSection>, ISectionName, IConfigObject<TSection>, new()
    {
        var sectionName = SectionName(TSection.SectionName, expression);

        var json = JsonSerializer.Serialize(value);

        return new ConfigAsCode.Entry<TSection>(
            new ConfigAsCode.Path(sectionName),
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
            new ConfigAsCode.Path(sectionName),
            new ConfigAsCode.Manual(),
            TSection.ValidateConfiguration);
    }


    //Entry
    public static ConfigAsCode.Entry<TSection> Value<TSection, TValue>(
        this ConfigAsCode.Entry<TSection> entry,
        Expression<Func<TSection, TValue>> expression,
        TValue value)
        where TSection : ConfigSection<TSection>, ISectionName, new()
    {
        var sectionName = SectionName(TSection.SectionName, expression);

        var json = JsonSerializer.Serialize(value);

        entry.Entries.Add(new ConfigAsCode.Path(sectionName), new ConfigAsCode.Value(json));

        return entry;
    }

    public static ConfigAsCode.Entry<TSection> Manual<TSection, TValue>(
        this ConfigAsCode.Entry<TSection> entry,
        Expression<Func<TSection, TValue>> expression)
        where TSection : ConfigSection<TSection>, ISectionName, new()
    {
        var sectionName = SectionName(TSection.SectionName, expression);

        entry.Entries.Add(new ConfigAsCode.Path(sectionName), new ConfigAsCode.Manual());

        return entry;
    }

    public static ConfigAsCode.Entry<TSection> Reference<TSection, TValue>(
        this ConfigAsCode.Entry<TSection> entry,
        Expression<Func<TSection, TValue>> expression,
        string path)
        where TSection : ConfigSection<TSection>, ISectionName, new()
    {
        var sectionName = SectionName(TSection.SectionName, expression);

        entry.Entries.Add(new ConfigAsCode.Path(sectionName), new ConfigAsCode.Reference(new ConfigAsCode.Path(path)));

        return entry;
    }

    private static string SectionName<T>(string sectionName, Expression<T> expression)
    {
        var member = expression.Body as MemberExpression;
        var parameterName = member!.Member.Name;

        return $"{sectionName}:{parameterName}";
    }
}