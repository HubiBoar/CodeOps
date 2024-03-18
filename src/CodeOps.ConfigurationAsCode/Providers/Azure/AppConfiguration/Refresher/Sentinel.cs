using Definit.Configuration;
using Definit.Validation.FluentValidation;

namespace CodeOps.ConfigurationAsCode.Azure;

public sealed class Sentinel : ConfigValue<Sentinel, int, IsNotNull<int>>
{
    public static string Name = "Sentinel";

    protected override string SectionName { get; } = Name;
}