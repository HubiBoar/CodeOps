using CodeOps.ArgumentAsCode;
using CodeOps.InfrastructureAsCode;

namespace Examples.WebApp;

internal sealed partial class Environment :
    ArgumentAsCode.IEntry<InfraAsCode.Enabled>
{
    public ArgumentAsCode.Entry<InfraAsCode.Enabled> ArgumentAsCode(ArgumentAsCode.Context<InfraAsCode.Enabled> context)
    {
        return MatchEnvironment(
            prod =>
                context                    
                    .DefaultValue(new InfraAsCode.Enabled(false))
                    .FromArgs<InfraAsCode.EnabledArgument>(Args)
                    .FromConfig<InfraAsCode.EnabledArgument>(Builder.Configuration),
            test =>
                context
                    .DefaultValue(new InfraAsCode.Enabled(false))
                    .FromArgs<InfraAsCode.EnabledArgument>(Args)
                    .FromConfig<InfraAsCode.EnabledArgument>(Builder.Configuration));
    }
}