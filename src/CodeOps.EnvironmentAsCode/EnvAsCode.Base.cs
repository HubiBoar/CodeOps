using CodeOps.ArgumentAsCode;
using Definit.Validation.FluentValidation;
using Microsoft.Extensions.Configuration;

namespace CodeOps.EnvironmentAsCode;

public static partial class EnvAsCode
{
    public sealed record NameArg(string Value) : ArgAsCode.IArgument<NameArg, string, IsNotEmpty>
    {
        public static string SectionName { get; } = "Environment";

        public static string ArgumentShortcut => "env";

        public static string ArgumentFullName => "environment";

        public static NameArg Map(string value) => new (value);
    }

    public sealed record ModeArg(bool Value) : ArgAsCode.IArgument<ModeArg, bool, IsNotNull<bool>>
    {
        public static string SectionName { get; } = "Mode";

        public static string ArgumentShortcut => "m";

        public static string ArgumentFullName => "mode";

        public static ModeArg Map(bool value) => new (value);
    }

    public abstract class Base
    {
        public abstract Name Name { get; }

        public abstract class ConfigArgs :
            Base,
            ArgAsCode.IEntry<NameArg>,
            ArgAsCode.IEntry<ModeArg>
        {
            public override Name Name { get; }
            protected int EnvironmentIndex { get; }
            protected IReadOnlyCollection<Name> Environments { get; }
            protected IConfiguration Configuration { get; }
            protected string[] Args { get; }
            protected string DefaultName { get; }

            protected ConfigArgs(IConfiguration configuration, string[] args, IReadOnlyCollection<Name> environments, string defaultName) : base()
            {
                Configuration = configuration;
                Args = args;
                DefaultName = defaultName;

                Environments = environments;
                (EnvironmentIndex, Name) = GetParameters(this.GetArgument<NameArg>());
            }

            protected virtual (int Index, Name Name) GetParameters(NameArg environment)
            {
                var index = Environments.ToList().FindIndex(x => x.Value == environment.Value);

                if (index == -1)
                {
                    throw new Exception("CodeOps.Environment: Does not match any of environments");
                }

                var environmentName = Environments.ElementAt(index);

                return (index, environmentName);
            }

            public virtual ArgAsCode.Entry<NameArg> ArgumentAsCode(ArgAsCode.Context<NameArg> context) => context
                .DefaultValue(new NameArg(DefaultName))
                    .FromArgs<NameArg>(Args)
                    .FromConfig<NameArg>(Configuration);

            public virtual ArgAsCode.Entry<ModeArg> ArgumentAsCode(ArgAsCode.Context<ModeArg> context) => context
                .DefaultValue(new ModeArg(false))
                    .FromArgs<ModeArg>(Args)
                    .FromConfig<ModeArg>(Configuration);
 
        }
    }
}