using Definit.Validation;
using OneOf;
using OneOf.Else;

namespace CodeOps.EnvironmentAsCode;

public static partial class ArgumentAsCode
{   
    public record struct Next();

    public interface IEntry<TArgument>
        where TArgument : notnull
    {
        public Entry<TArgument> ArgumentAsCode(Context<TArgument> context);

        public TArgument GetArgument()
        {
            return ArgumentAsCode(new Context<TArgument>()).GetValue();
        }
    }

    public sealed partial class Context<TArgument>
        where TArgument : notnull
    {
        internal Context()
        {
        }

        public Entry<TArgument> DefaultValue(TArgument argument)
        {
            return new Entry<TArgument>(argument);
        }
    }

    public sealed partial class Entry<TArgument>
        where TArgument : notnull
    {
        private readonly List<Func<OneOf<TArgument, Next, ValidationErrors>>> _factories = [];

        private readonly TArgument _defaultValue;

        internal Entry(TArgument defaultValue)
        {
            _defaultValue = defaultValue;
        }

        public static implicit operator Entry<TArgument>(TArgument entry)
        {
            return new Entry<TArgument>(entry);
        }

        public TArgument GetValue()
        {
            foreach(var factory in _factories)
            {
                if(factory().Is(out TArgument args))
                {
                    return args;
                }
            }

            return _defaultValue;
        }
    }
}