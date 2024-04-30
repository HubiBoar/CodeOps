using System.Text.Json;
using Definit.Configuration;
using Definit.Validation;
using OneOf;
using OneOf.Types;

namespace CodeOps.ConfigurationAsCode;

public static class ValueExtensions
{
    public static ValidationResult AddConfig<TSection>(
        this ConfigAsCode.Builder builder,        
        ConfigAsCode.IEntry<TSection> configAsCode)
        where TSection : IConfigValue, new()
    {
        return builder.AddConfig(TSection.Register, configAsCode);
    }

    public static ConfigAsCode.Entry<TSection> Value<TSection, TValue>(
        this ConfigAsCode.Context<TSection> _,
        TValue value)
        where TSection : IConfigValue<TValue>, new()
    {
        var json = JsonSerializer.Serialize(value);

        return new ConfigAsCode.Entry<TSection>(
            new ConfigAsCode.Path(TSection.SectionName),
            new ConfigAsCode.Value(json),
            TSection.ValidateConfiguration);
    }

    public static ConfigAsCode.Entry<TSection> Manual<TSection>(
        this ConfigAsCode.Context<TSection> _)
        where TSection : IConfigValue, new()
    {
        return new ConfigAsCode.Entry<TSection>(
            new ConfigAsCode.Path(TSection.SectionName),
            new ConfigAsCode.Manual(),
            TSection.ValidateConfiguration);
    }

    public static ConfigAsCode.Entry<TSection> Reference<TSection>(
        this ConfigAsCode.Context<TSection> _,
        string path)
        where TSection : IConfigValue, new()
    {
        return new ConfigAsCode.Entry<TSection>(
            new ConfigAsCode.Path(TSection.SectionName),
            new ConfigAsCode.Reference(new ConfigAsCode.Path(path)),
            TSection.ValidateConfiguration);
    }
}
