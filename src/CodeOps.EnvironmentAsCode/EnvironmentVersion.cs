namespace CodeOps.EnvironmentAsCode;

public static partial class EnvAsCode
{
    public interface IVersion
    {
        public static abstract string Name { get; }
    }

    public sealed record Name(string Value);

    public sealed class Environment<T>
        where T : IVersion
    {
        
        public Name EnvironmentName { get; }
        
        public string Name { get; }

        internal Environment()
        {
            Name = T.Name;
            EnvironmentName = new Name(Name);
        }
    }

}