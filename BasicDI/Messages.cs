namespace BasicDI;

internal static class Messages
{
    internal const string IncompatibleResolvingType = ResolvingTypeCannotBeBound + " Resolving type is not assignable to dependency type.";
    internal const string RegisteredTypeNotConcreteClass = "The registered type {0} must be a concrete class type.";
    internal const string ResolvingTypeCannotBeBound = "Resolving type {0} can't be bound to dependency type {1}.";
    internal const string ResolvingTypeNotConcreteClass = ResolvingTypeCannotBeBound + " Resolving type must be a concrete class type.";
}