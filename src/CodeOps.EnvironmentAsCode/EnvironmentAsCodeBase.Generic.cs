using Microsoft.Extensions.Configuration;

namespace CodeOps.EnvironmentAsCode;

public static partial class EnvAsCode
{
    public interface IEnvironment
    {
        public static abstract IReadOnlyCollection<Name> Environments { get; }
    }

    public abstract class Base<T0, T1> : 
        Base.ConfigArgs,
        IEnvironment
            where T0 : IVersion
            where T1 : IVersion
    {
        public new static IReadOnlyCollection<Name> Environments { get; } =
        [
            new Name(T0.Name),
            new Name(T1.Name)
        ];
        
        protected Base(IConfiguration configuration, string[] args) : base(configuration, args, Environments, T0.Name)
        {
        }

        public TReturnValue MatchEnvironment<TReturnValue>(
            Func<Environment<T0>, TReturnValue> onT0,   
            Func<Environment<T1>, TReturnValue> onT1)
        {
            return EnvironmentIndex switch
            {
                0 => onT0(new Environment<T0>()),
                _ => onT1(new Environment<T1>())
            };
        }
    }

    public abstract class Base<T0, T1, T2> :
        Base.ConfigArgs,
        IEnvironment
            where T0 : IVersion
            where T1 : IVersion
            where T2 : IVersion
    {
        public new static IReadOnlyCollection<Name> Environments { get; } = 
        [
            new Name(T0.Name),
            new Name(T1.Name),
            new Name(T2.Name)
        ];

        protected Base(IConfiguration configuration, string[] args) : base(configuration, args, Environments, T0.Name)
        {
        }

        public TReturnValue MatchEnvironment<TReturnValue>(
            Func<Environment<T0>, TReturnValue> onT0,
            Func<Environment<T1>, TReturnValue> onT1,
            Func<Environment<T2>, TReturnValue> onT2)
        {
            return EnvironmentIndex switch
            {
                0 => onT0(new Environment<T0>()),
                1 => onT1(new Environment<T1>()),
                _ => onT2(new Environment<T2>()),
            };
        }
    }

    public abstract class Base<T0, T1, T2, T3> :
        Base.ConfigArgs,
        IEnvironment
            where T0 : IVersion
            where T1 : IVersion
            where T2 : IVersion
            where T3 : IVersion
    {
        public new static IReadOnlyCollection<Name> Environments { get; } =
        [
            new Name(T0.Name),
            new Name(T1.Name),
            new Name(T2.Name),
            new Name(T3.Name)
        ];

        protected Base(IConfiguration configuration, string[] args) : base(configuration, args, Environments, T0.Name)
        {
        }

        public TReturnValue MatchEnvironment<TReturnValue>(
            Func<Environment<T0>, TReturnValue> onT0,
            Func<Environment<T1>, TReturnValue> onT1,
            Func<Environment<T2>, TReturnValue> onT2,
            Func<Environment<T3>, TReturnValue> onT3)
        {
            return EnvironmentIndex switch
            {
                0 => onT0(new Environment<T0>()),
                1 => onT1(new Environment<T1>()),
                2 => onT2(new Environment<T2>()),
                _ => onT3(new Environment<T3>()),
            };
        }
    }
}