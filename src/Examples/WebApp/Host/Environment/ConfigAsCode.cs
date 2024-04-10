using CodeOps.ConfigurationAsCode;
using CodeOps.ArgumentAsCode;

namespace Examples.WebApp;

internal sealed partial class Environment :
    ArgumentAsCode.IEntry<ConfigAsCode.Enabled>
{
    public ArgumentAsCode.Entry<ConfigAsCode.Enabled> ArgumentAsCode(ArgumentAsCode.Context<ConfigAsCode.Enabled> context)
    {
        return MatchEnvironment(
            prod =>
                context                    
                    .DefaultValue(new ConfigAsCode.Enabled(false))
                    .FromArgs<ConfigAsCode.EnabledArgument>(Args)
                    .FromConfig<ConfigAsCode.EnabledArgument>(Builder.Configuration),
            test =>
                context
                    .DefaultValue(new ConfigAsCode.Enabled(false))
                    .FromArgs<ConfigAsCode.EnabledArgument>(Args)
                    .FromConfig<ConfigAsCode.EnabledArgument>(Builder.Configuration));
    }
}