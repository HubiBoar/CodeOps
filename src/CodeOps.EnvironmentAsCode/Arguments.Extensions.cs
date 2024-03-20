using System.Text.Json;
using Definit.Validation;
using Microsoft.Extensions.Configuration;
using OneOf;
using OneOf.Types;

namespace CodeOps.EnvironmentAsCode;

public static partial class ArgumentAsCode
{
    public static OneOf<TValue, Next, ValidationErrors> ParseArgument<TValue>(
        string[] args,
        string shortcut,
        string fullName,
        Func<string, OneOf<TValue, ValidationErrors>> factory)
        where TValue : notnull
    {
        shortcut = $"-{shortcut}=";
        fullName = $"--{fullName}=";

        var foundArgsValue = args.SingleOrDefault(arg => arg.StartsWith(fullName) || arg.StartsWith(shortcut));

        if(foundArgsValue is null)
        {
            return new Next();
        }

        var argsValue = foundArgsValue
            ?.Replace(fullName, string.Empty)
            ?.Replace(shortcut, string.Empty)!;

        if(argsValue is null)
        {
            return ValidationErrors.Null($"Args Argument is missing: [{fullName}][{shortcut}]");
        }

        return factory(argsValue)
            .Match<OneOf<TValue, Next, ValidationErrors>>(
                value => value,
                error => error);
    }

    public static OneOf<TValue, Next, ValidationErrors> ParseArgument<TValue>(
        string[] args,
        string shortcut,
        string fullName,
        Func<TValue, OneOf<Success, ValidationErrors>> validate)
        where TValue : notnull
    {
        return ParseArgument(args, shortcut, fullName, argsValue => 
        {
            var value = JsonSerializer.Deserialize<TValue>(argsValue);

            if(value is null)
            {
                return ValidationErrors.Null($"Args Argument coudn't be deserialized from json: [{fullName}][{shortcut}]");
            }

            return validate(value)
                .Match<OneOf<TValue, ValidationErrors>>(
                    success => value,
                    error => error);
        });
    }

    public static TArgument GetArgument<TArgument>(this IEntry<TArgument> entry)
        where TArgument : notnull
    {
        return entry.GetArgument();
    }

    public static Entry<TArgument> FromArgs<TArgument>(this Context<TArgument> context, string[] args)
        where TArgument : notnull, IArgsArgument<TArgument, TArgument>
    {
        return context.FromArgs<TArgument>(args);
    }

    public static Entry<TArgument> FromConfig<TArgument>(this Context<TArgument> context, IConfiguration configuration)
        where TArgument : notnull, IConfigArgument<TArgument, TArgument>
    {
        return context.FromConfig<TArgument>(configuration);
    }

    public static Entry<TArgument> FromArgs<TArgument>(this Entry<TArgument> entry, string[] args)
        where TArgument : IArgsArgument<TArgument, TArgument>
    {
        return entry.FromArgs<TArgument>(args);
    }

    public static Entry<TArgument> FromConfig<TArgument>(this Entry<TArgument> entry, IConfiguration configuration)
        where TArgument : notnull, IConfigArgument<TArgument, TArgument>
    {
        return entry.FromConfig<TArgument>(configuration);
    }
}
