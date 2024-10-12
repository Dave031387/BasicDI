namespace BasicDI;

/// <summary>
/// An interface that defines a simple dependency injection container.
/// </summary>
public interface IContainer
{
    /// <summary>
    /// Create a new <see cref="Dependency{T}" /> object to be used for binding the specified
    /// dependency type and registering it with the dependency injection container.
    /// </summary>
    /// <typeparam name="T">
    /// The dependency type.
    /// </typeparam>
    /// <returns>
    /// A new <see cref="Dependency{T}" /> object representing the dependency.
    /// </returns>
    ICanBindTo<T> Bind<T>() where T : class;

    /// <summary>
    /// Create a new scope and add it to the scope list.
    /// </summary>
    /// <returns>
    /// An <see cref="IScope" /> object for managing the scoped dependency lifetime.
    /// </returns>
    IScope CreateScope();

    /// <summary>
    /// Get the <see cref="IDependency{T}" /> instance for the specified dependency type.
    /// </summary>
    /// <typeparam name="T">
    /// The dependency type to look for.
    /// </typeparam>
    /// <returns>
    /// The <see cref="IDependency{T}" /> instance for the specified dependency type, or
    /// <see langword="null" /> if the dependency hasn't been registered in the container.
    /// </returns>
    IDependency<T>? GetDependency<T>() where T : class;

    /// <summary>
    /// Create a new <see cref="Dependency{T}" /> instance to be used for registering the specified
    /// concrete dependency type with the dependency injection container.
    /// </summary>
    /// <typeparam name="T">
    /// The dependency type to register. Must be a concrete type. (not an abstract class or
    /// interface)
    /// </typeparam>
    /// <param name="factory">
    /// Optional factory delegate for creating instances of the dependency type.
    /// </param>
    /// <returns>
    /// A new <see cref="Dependency{T}" /> instance representing the dependency.
    /// </returns>
    /// <exception cref="DependencyInjectionException" />
    ICanSpecifyLifetime Register<T>(Func<T>? factory = null) where T : class;

    /// <summary>
    /// Get the resolving instance for the specified dependency type.
    /// </summary>
    /// <typeparam name="T">
    /// The dependency type that is to be resolved.
    /// </typeparam>
    /// <returns>
    /// An instance of the resolving type that was bound to the dependency type.
    /// </returns>
    /// <remarks>
    /// This method is invoked recursively until all nested dependencies of the given dependency
    /// type have been resolved.
    /// </remarks>
    /// <exception cref="DependencyInjectionException" />
    T Resolve<T>() where T : class;
}