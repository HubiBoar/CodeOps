using Definit.Configuration;
using Definit.Primitives;
using Definit.Results;
using Definit.Utils;
using Definit.Validation;
using Microsoft.Extensions.Configuration;
using OneOf;

namespace CodeOps.ArgumentAsCode;

public static partial class ArgAsCode
{   
    public interface IArgument<TSelf, TValue, TValidationMethod> : IArgument<TSelf, TSelf, TValue, TValidationMethod>
        where TSelf : IArgument<TSelf, TValue, TValidationMethod>
        where TValue : notnull
        where TValidationMethod : IValidate<TValue>
    {
    }

    public interface IArgument<TSelf, TArgument, TValue, TValidationMethod> : IConfigArgument<TSelf, TArgument, TValue, TValidationMethod>, IArgsArgument<TSelf, TArgument, TValue, TValidationMethod>
        where TSelf : IArgument<TSelf, TArgument, TValue, TValidationMethod>
        where TArgument : notnull
        where TValue : notnull
        where TValidationMethod : IValidate<TValue>
    {
    }

    public interface IArgsArgument<TSelf, TArgument, TValue, TValidationMethod> : IArgumentMap<TValue, TArgument>, IArgsArgument<TSelf, TArgument>
        where TSelf : IArgsArgument<TSelf, TArgument, TValue, TValidationMethod>
        where TArgument : notnull
        where TValue : notnull
        where TValidationMethod : IValidate<TValue>
    {
        static OneOf<TArgument, Next, ValidationErrors, Error> IArgsArgument<TSelf, TArgument>.ParseArgs<T>(string[] args)
        {
            var parsedValue = ParseArgument<TValue>(args, T.ArgumentShortcut, T.ArgumentFullName, value => 
                value
                    .IsValid<TValue, TValidationMethod>()
                    .Match<ValidationResult>(
                        success => Success.Instance,
                        error => error,
                        error => error));

            return parsedValue
                .Match<OneOf<TArgument, Next, ValidationErrors, Error>>(
                    val => T.Map(val),
                    next => next,
                    error => error,
                    error => error);
        }
    }

    public interface IArgsArgument<TSelf, TValue, TValidationMethod> : IArgsArgument<TSelf, TSelf, TValue, TValidationMethod>
        where TSelf : IArgsArgument<TSelf, TValue, TValidationMethod>
        where TValue : notnull
        where TValidationMethod : IValidate<TValue>
    {
    }

    public interface IArgsArgument<TSelf, TArgument>
        where TSelf : IArgsArgument<TSelf, TArgument>
    {
        public static abstract string ArgumentShortcut { get; }
        public static abstract string ArgumentFullName { get; }

        public static abstract OneOf<TArgument, Next, ValidationErrors, Error> ParseArgs<T>(string[] args)
            where T : TSelf;
    }

    public interface IConfigArgument<TSelf, TArgument, TValue, TValidationMethod> : IArgumentMap<TValue, TArgument>, IConfigArgument<TSelf, TArgument>
        where TSelf : IConfigArgument<TSelf, TArgument, TValue, TValidationMethod>
        where TArgument : notnull
        where TValue : notnull
        where TValidationMethod : IValidate<TValue>
    {
        static OneOf<TArgument, Next, ValidationErrors, Error> IConfigArgument<TSelf, TArgument>.ParseConfig<T>(IConfiguration configuration)
        {
            var sectionName = T.SectionName;
            var section = configuration.GetSection(sectionName);
            if(section.Exists() == false)
            {
                return new Next();
            }

            var value = section.Get<TValue>();
            if (value is null)
            {
                return ValidationErrors.Null(typeof(TValue).GetTypeVerboseName());
            }

            return value
                .IsValid<TValue, TValidationMethod>()
                .Match<OneOf<TArgument, Next, ValidationErrors, Error>>(
                    valid => T.Map(value),
                    error => error,
                    error => error);
        }
    }

    public interface IConfigArgument<TSelf, TValue, TValidationMethod> : IConfigArgument<TSelf, TSelf, TValue, TValidationMethod>
        where TSelf : IConfigArgument<TSelf, TValue, TValidationMethod>
        where TValue : notnull
        where TValidationMethod : IValidate<TValue>
    {
    }

    public interface IConfigArgument<TSelf, TArgument> : ISectionName
        where TSelf : IConfigArgument<TSelf, TArgument>
        where TArgument : notnull    
    {
        public static abstract OneOf<TArgument, Next, ValidationErrors, Error> ParseConfig<T>(IConfiguration configuration)
            where T : TSelf;
    }

    public interface IArgumentMap<TValue, TArgument>
        where TValue : notnull
    {
        public static abstract TArgument Map(TValue value);
    }
}