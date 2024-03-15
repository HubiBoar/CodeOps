using Definit.Configuration;
using Definit.Validation.FluentValidation;

namespace CodeOps.ConfigurationAsCode.Providers.Azure;

//In milliseconds
internal sealed class ConfigurationRefresherDelay :
    ConfigValue<ConfigurationRefresherDelay, int, IsNotNull<int>>
{
    protected override string SectionName { get; } = "ConfigurationRefresherDelay";
}