namespace CodeOps.EnvironmentAsCode;

public abstract class EnvironmentAsCode<T0, T1> : EnvironmentAsCodeBase<EnvironmentAsCode<T0, T1>>, IEnvironmentAsCode
    where T0 : IEnvironmentVersion
    where T1 : IEnvironmentVersion
{
    public static IReadOnlyCollection<EnvironmentName> Environments { get; } =
    [
        new EnvironmentName(T0.Name),
        new EnvironmentName(T1.Name)
    ];
    
    protected EnvironmentAsCode(EnvironmentAsCodeName environment) : base(environment)
    {
    }

    public TReturnValue MatchEnvironment<TReturnValue>(
        Func<Environment<T0>, TReturnValue> onT0,   
        Func<Environment<T1>, TReturnValue> onT1)
    {
        return Parameters.Index switch
        {
            0 => onT0(new Environment<T0>()),
            _ => onT1(new Environment<T1>())
        };
    }
}

public abstract class EnvironmentAsCode<T0, T1, T2> : EnvironmentAsCodeBase<EnvironmentAsCode<T0, T1, T2>>, IEnvironmentAsCode
    where T0 : IEnvironmentVersion
    where T1 : IEnvironmentVersion
    where T2 : IEnvironmentVersion
{
    public static IReadOnlyCollection<EnvironmentName> Environments { get; } = 
    [
        new EnvironmentName(T0.Name),
        new EnvironmentName(T1.Name),
        new EnvironmentName(T2.Name)
    ];
    
    protected EnvironmentAsCode(EnvironmentAsCodeName environment) : base(environment)
    {
    }

    public TReturnValue MatchEnvironment<TReturnValue>(
        Func<Environment<T0>, TReturnValue> onT0,
        Func<Environment<T1>, TReturnValue> onT1,
        Func<Environment<T2>, TReturnValue> onT2)
    {
        return Parameters.Index switch
        {
            0 => onT0(new Environment<T0>()),
            1 => onT1(new Environment<T1>()),
            _ => onT2(new Environment<T2>()),
        };
    }
}


public abstract class EnvironmentAsCode<T0, T1, T2, T3> : EnvironmentAsCodeBase<EnvironmentAsCode<T0, T1, T2, T3>>, IEnvironmentAsCode
    where T0 : IEnvironmentVersion
    where T1 : IEnvironmentVersion
    where T2 : IEnvironmentVersion
    where T3 : IEnvironmentVersion
{
    public static IReadOnlyCollection<EnvironmentName> Environments { get; } =
    [
        new EnvironmentName(T0.Name),
        new EnvironmentName(T1.Name),
        new EnvironmentName(T2.Name),
        new EnvironmentName(T3.Name)
    ];
    
    protected EnvironmentAsCode(EnvironmentAsCodeName environment) : base(environment)
    {
    }

    public TReturnValue MatchEnvironment<TReturnValue>(
        Func<Environment<T0>, TReturnValue> onT0,
        Func<Environment<T1>, TReturnValue> onT1,
        Func<Environment<T2>, TReturnValue> onT2,
        Func<Environment<T3>, TReturnValue> onT3)
    {
        return Parameters.Index switch
        {
            0 => onT0(new Environment<T0>()),
            1 => onT1(new Environment<T1>()),
            2 => onT2(new Environment<T2>()),
            _ => onT3(new Environment<T3>()),
        };
    }
}