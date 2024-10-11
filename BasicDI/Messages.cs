namespace BasicDI;

internal static class Messages
{
    internal const string FailedToConstructResolvingObject = "An exception was thrown while trying to construct the resolving object for dependency type {0}.\nReason: {1}";
    internal const string IncompatibleResolvingType = ResolvingTypeCannotBeBound + " Resolving type is not assignable to dependency type.";
    internal const string InvalidLifetime = "Can't retrieve the resolving object for a dependency having an invalid lifetime.";
    internal const string RegisteredTypeNotConcreteClass = "The registered type {0} must be a concrete class type.";
    internal const string ResolvingScopedDependencyOutsideOfScope = "Invalid attempt to resolve scoped dependency {0} outside of a scope.";
    internal const string ResolvingTypeCannotBeBound = "Resolving type {0} can't be bound to dependency type {1}.";
    internal const string ResolvingTypeNotConcreteClass = ResolvingTypeCannotBeBound + " Resolving type must be a concrete class type.";
    internal const string UnableToResolveDependency = "Unable to resolve dependency {0}.";
    internal const string UnableToResolveUnknownDependency = UnableToResolveDependency + " The dependency was never registered with the container.";
}