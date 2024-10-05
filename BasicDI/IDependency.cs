namespace BasicDI;

/// <summary>
/// An interface that defines a dependency.
/// </summary>
/// <typeparam name="T">
/// The type of the dependency.
/// </typeparam>
public interface IDependency<T> where T : class
{
    /// <summary>
    /// Gets the factory object used for creating instances of the resolving type.
    /// </summary>
    /// <remarks>
    /// <see langword="null" /> will be returned if no factory is defined for this dependency.
    /// </remarks>
    Func<T>? Factory
    {
        get;
    }

    /// <summary>
    /// Gets the lifetime of the dependency.
    /// </summary>
    /// <remarks>
    /// <see cref="DependencyLifetime.Undefined" /> will be returned if no lifetime has been set for
    /// this dependency.
    /// </remarks>
    DependencyLifetime Lifetime
    {
        get;
    }

    /// <summary>
    /// Gets an instance of the resolving object for this dependency.
    /// </summary>
    /// <remarks>
    /// <see langword="null" /> will be returned if this dependency isn't a singleton.
    /// </remarks>
    T? ResolvingObject
    {
        get;
    }

    /// <summary>
    /// Gets the type of the resolving object for this dependency.
    /// </summary>
    /// <remarks>
    /// This property will return <see langword="typeof" />(<see cref="object" />) if no resolving
    /// type has been specified.
    /// </remarks>
    Type ResolvingType
    {
        get;
    }

    /// <summary>
    /// Gets the type of this dependency.
    /// </summary>
    Type Type
    {
        get;
    }
}