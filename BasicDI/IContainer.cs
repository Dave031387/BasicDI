namespace BasicDI;

using System;

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
    /// The type of the dependency object.
    /// </typeparam>
    /// <returns>
    /// A new <see cref="Dependency{T}" /> object representing the dependency.
    /// </returns>
    ICanBindTo<T> Bind<T>() where T : class;

    /// <summary>
    /// Create a new scope and add it to the scope list.
    /// </summary>
    /// <returns>
    /// An <see cref="IScope" /> object representing the scoped dependency lifetime.
    /// </returns>
    IScope CreateScope();

    /// <summary>
    /// Get the dependency object for the specified dependency type.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the dependency to get.
    /// </typeparam>
    /// <returns>
    /// The <see cref="Dependency{T}" /> object for the specified dependency type, or
    /// <see langword="null" /> if the dependency hasn't been registered in the container.
    /// </returns>
    IDependency<T>? GetDependency<T>() where T : class;

    /// <summary>
    /// Create a new <see cref="Dependency{T}" /> object to be used for registering the specified
    /// concrete dependency type with the dependency injection container.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the dependency object. Must be a concrete type. (not an interface)
    /// </typeparam>
    /// <param name="factory">
    /// Optional factory delegate for creating instances of the dependency type.
    /// </param>
    /// <returns>
    /// A new <see cref="Dependency{T}" /> object representing the dependency.
    /// </returns>
    /// <exception cref="InvalidOperationException" />
    ICanSpecifyLifetime Register<T>(Func<T>? factory = null) where T : class;

    /// <summary>
    /// Resolve the specified dependency type.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the dependency that is to be resolved.
    /// </typeparam>
    /// <returns>
    /// An instance of the resolving type that was bound to the dependency type.
    /// </returns>
    /// <exception cref="InvalidOperationException" />
    T Resolve<T>() where T : class;
}