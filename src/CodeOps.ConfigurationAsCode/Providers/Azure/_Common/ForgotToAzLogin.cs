namespace Momolith.Azure.Common;

public class ForgotToAzLogin : Exception
{
    public ForgotToAzLogin(string message)
        : base(message)
    {
    }
}