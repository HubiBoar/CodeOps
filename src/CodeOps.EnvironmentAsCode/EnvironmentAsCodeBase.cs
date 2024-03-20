using Definit.Configuration;
using Definit.Validation.FluentValidation;
using Microsoft.Extensions.Configuration;

namespace CodeOps.EnvironmentAsCode;

public sealed record EnvironmentAsCodeName(string EnvironmentName);

public sealed record EnvironmentParameters(EnvironmentName Name, int Index);

public interface IEnvironmentAsCode
{
    public static abstract IReadOnlyCollection<EnvironmentName> Environments { get; }
}

public abstract class EnvironmentAsCodeBase<TEnvs>
    where TEnvs : IEnvironmentAsCode
{
    public EnvironmentName Name => Parameters.Name;

    protected EnvironmentParameters Parameters { get; }

    protected EnvironmentAsCodeBase(EnvironmentAsCodeName environment)
    {
        var index = TEnvs.Environments.ToList().FindIndex(x => x.Name == environment.EnvironmentName);

        if (index == -1)
        {
            throw new Exception("CodeOps.Environment: Does not match any of environments");
        }

        var environmentName = TEnvs.Environments.ElementAt(index);

        Parameters = new EnvironmentParameters(environmentName, index);
    }
    
    protected static EnvironmentAsCodeName GetSettingFromConfiguration(IConfiguration configuration)
    {
        return EnvironmentSetting.Create(configuration).Match(
            valid => new EnvironmentAsCodeName(valid.ValidValue),
            errors => throw errors.ToException());
    }
    
    private sealed class EnvironmentSetting : ConfigValue<EnvironmentSetting, string, IsNotEmpty>
    {
        protected override string SectionName { get; } = "Environment";
    }
}