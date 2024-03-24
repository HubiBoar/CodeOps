using CodeOps.InfrastructureAsCode;

namespace CodeOps.DeploymentAsCode;

public interface IDeployCode : InfraAsCode.IComponent
{
    public Task DeployCode();
}