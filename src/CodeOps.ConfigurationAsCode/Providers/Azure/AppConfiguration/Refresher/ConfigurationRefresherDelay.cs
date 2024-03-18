using Definit.Configuration;
using Definit.Validation.FluentValidation;

namespace CodeOps.ConfigurationAsCode.Azure;

//In milliseconds
public sealed class ConfigurationRefresherDelay :
    ConfigValue<ConfigurationRefresherDelay, int, IsNotNull<int>>
{
    protected override string SectionName { get; } = "ConfigurationRefresherDelay";
}