using System.Text.Json;
using CodeOps.InfrastructureAsCode;
using Definit.Validation;
using Definit.Validation.FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneOf;
using CodeOps.ArgumentAsCode;

namespace CodeOps.ConfigurationAsCode;

public static partial class ConfigAsCode
{
    public delegate ValidationResult RegisterConfig(IServiceCollection services, IConfiguration configuration);
    
    public struct EnabledArgument : ArgAsCode.IArgument<EnabledArgument, Enabled, bool, IsNotNull<bool>>
    {
        public static string SectionName => "ConfigAsCode";

        public static string ArgumentShortcut => "cac";

        public static string ArgumentFullName => "config-as-code";

        public static Enabled Map(bool value) => new (value);
    }

    public sealed record Enabled(bool IsEnabled);

    public interface IEntry<TOptions>
    {
        public Entry<TOptions> ConfigurationAsCode(Context<TOptions> context);
    }

    public interface ISource : InfraAsCode.IComponent
    {
        public void Register(IServiceCollection services, IConfigurationManager config);

        public void UploadValues(IReadOnlyDictionary<Path, OneOf<Value, FiltersEnabledFor, FeatureFlag, Reference>> entries);

        public string GetValue(Path path);
    }

    public sealed class Context<TOptions>
    {
        internal Context()
        {
        }
    }

    public sealed record Entry<TOptions>(
        Dictionary<Path, OneOf<Value, FiltersEnabledFor, FeatureFlag, Manual, Reference>> Entries,
        Func<IConfiguration, ValidationResult> Validation)
    {
        public Entry(
            Path sectionName,
            OneOf<Value, FiltersEnabledFor, FeatureFlag, Manual, Reference> sectionValue,
            Func<IConfiguration, ValidationResult> validation) :
            this(
                new Dictionary<Path, OneOf<Value,  FiltersEnabledFor, FeatureFlag, Manual, Reference>>
                {
                    [sectionName] = sectionValue
                },
                validation)
        {
        }
    }

    public sealed record Path(string Value)
    {
        public bool IsFeature { get; } = Value.StartsWith("FeatureManagement:");
    }

    public sealed record Value(string Val);
    public sealed record FiltersEnabledFor(IReadOnlyCollection<FeatureFilter> EnabledFor)
    {
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
    public sealed record FeatureFilter(string Name, object Parameters);
    public sealed record FeatureFlag(bool Enabled)
    {
        public string ToJson()
        {
            return Enabled.ToString();
        }
    }
    public sealed record Manual();
    public sealed record Reference(Path Path);
}