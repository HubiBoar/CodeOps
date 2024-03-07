using System.Linq.Expressions;
using System.Text.Json;
using Definit.Configuration;

namespace CodeOps.ConfigurationAsCode;

public static class ValueExtensions
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
