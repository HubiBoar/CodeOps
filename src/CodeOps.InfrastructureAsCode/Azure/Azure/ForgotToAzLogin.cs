namespace CodeOps.InfrastructureAsCode.Azure;

public sealed class ForgotToAzLogin : Exception
{
    public ForgotToAzLogin(string message)
        : base(message)
    {
    }
}