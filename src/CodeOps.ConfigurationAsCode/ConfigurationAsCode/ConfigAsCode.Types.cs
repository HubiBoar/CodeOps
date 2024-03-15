using Definit.Configuration;
using Definit.Validation;
using Microsoft.Extensions.Configuration;
using OneOf;
using Success = OneOf.Types.Success;

namespace CodeOps.ConfigurationAsCode;

public sealed partial class ConfigAsCode
{
    public interface ISetup<TSection>
        where TSection : ISectionName
    {
        public Entry<TSection> ConfigurationAsCode(Context<TSection> context);
    }

    public sealed class Context<TSection>
        where TSection : ISectionName
    {
        internal Context()
        {
        }
    }

    public sealed record Entry<TSection>(
        Dictionary<string, OneOf<Value, Manual, Reference>> Entries,
        Func<IConfiguration, OneOf<Success, ValidationErrors>> Validation)
        where TSection : ISectionName
    {
        public Entry(
            string sectionName,
            OneOf<Value, Manual, Reference> sectionValue,
            Func<IConfiguration, OneOf<Success, ValidationErrors>> validation) :
            this(
                new Dictionary<string, OneOf<Value, Manual, Reference>>
                {
                    [sectionName] = sectionValue
                },
                validation)
        {
        }
    }

    public sealed record Value(string Val);
    public sealed record Manual();
    public sealed record Reference(string Path);
}