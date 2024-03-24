namespace CodeOps.EnvironmentAsCode;

public interface IEnvironmentVersion
{
    public static abstract string Name { get; }
}

public sealed class EnvironmentName
{
    public string Name { get; }

    internal EnvironmentName(string name)
    {
        Name = name;
    }
}

public sealed class Environment<T>
    where T : IEnvironmentVersion
{
    
    public EnvironmentName EnvironmentName { get; }
    
    public string Name { get; }

    internal Environment()
    {
        Name = T.Name;
        EnvironmentName = new EnvironmentName(Name);
    }
}

public static class TaskExtensions
{
    public static Task<T> AsTask<T>(this T obj)
    {
        return Task.FromResult(obj);
    }
}