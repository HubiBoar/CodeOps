using Definit.Results;
using Definit.Validation;
using Microsoft.Extensions.Configuration;
using OneOf;

namespace CodeOps.ArgumentAsCode;

public static partial class ArgAsCode
{
    public sealed partial class Entry<TArgument>
        where TArgument : notnull
    {
        public Entry<TArgument> FromArgs<TArgs>(string[] args)
            where TArgs : IArgsArgument<TArgs, TArgument>
        {
            return AddFactory(() => TArgs.ParseArgs<TArgs>(args));
        }
        public Entry<TArgument> FromConfig<TArgs>(IConfiguration configuration)
            where TArgs : IConfigArgument<TArgs, TArgument>
        {
            return AddFactory(() => TArgs.ParseConfig<TArgs>(configuration));
        }

        public Entry<TArgument> FromArgs(string[] args, string shortcut, string fullName, Func<string, OneOf<TArgument, ValidationErrors, Error>> factory)
        {
            return AddFactory(() => ParseArgument(args, shortcut, fullName, factory));
        }

        public Entry<TArgument> FromArgs(string[] args, string shortcut, string fullName, Func<TArgument, ValidationResult> factory)
        {
            return AddFactory(() => ParseArgument(args, shortcut, fullName, factory));
        }

        public Entry<TArgument> FromConfig(IConfiguration configuration, Func<IConfiguration, OneOf<TArgument, Next, ValidationErrors, Error>> factory)
        {
            return AddFactory(() => factory(configuration));
        }

        public Entry<TArgument> AddFactory(Func<OneOf<TArgument, Next, ValidationErrors, Error>> factory)
        {
            _factories.Add(factory);
            return this;
        }
    }

    public static Task<T> AsTask<T>(this T value) => Task.FromResult(value);    
}