﻿using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BasicDI.Tests")]

namespace BasicDI;

/// <summary>
/// A simple dependency injection container.
/// </summary>
public class Container : IContainer
{
    /// <summary>
    /// The dependency mapping container.
    /// </summary>
    internal readonly Dictionary<Type, object> _dependencies = [];

    /// <summary>
    /// A list of dependency scopes that are currently active for this dependency injection
    /// container.
    /// </summary>
    internal readonly Dictionary<Guid, Scope> _scopes = [];

    /// <summary>
    /// A lazy initializer for the dependency injection container.
    /// </summary>
    private static readonly Lazy<Container> _lazy = new(static () => new Container());

    /// <summary>
    /// A lock object used to facilitate thread safety on operations against the dependency mapping
    /// container.
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// The default constructor is marked private to prevent the user from invoking it directly.
    /// </summary>
    private Container()
    {
    }

    /// <summary>
    /// Get the current dependency injection container.
    /// </summary>
    /// <remarks>
    /// This returns a thread safe singleton instance of the container.
    /// </remarks>
    public static Container Current => _lazy.Value;

    /// <summary>
    /// Get a test instance of the dependency injection container for use in unit tests.
    /// </summary>
    /// <remarks>
    /// This returns a new instance of the container each time it is called.
    /// </remarks>
    internal static Container TestInstance => new();

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
    public ICanBindTo<T> Bind<T>() where T : class => new Dependency<T>(this);

    /// <summary>
    /// Create a new scope and add it to the scope list.
    /// </summary>
    /// <returns>
    /// An <see cref="IScope" /> object representing the scoped dependency lifetime.
    /// </returns>
    public IScope CreateScope()
    {
        Scope scope = new(this);

        lock (_lock)
        {
            _scopes[scope.Guid] = scope;
        }

        return scope;
    }

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
    public IDependency<T>? GetDependency<T>() where T : class
    {
        lock (_lock)
        {
            if (_dependencies.TryGetValue(typeof(T), out object? dependency))
            {
                return (IDependency<T>)dependency;
            }
        }

        return null;
    }

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
    public ICanSpecifyLifetime Register<T>(Func<T>? factory = null) where T : class
    {
        Type type = typeof(T);

        if (type.IsClass && !type.IsAbstract)
        {
            Dependency<T> dependency = new(this)
            {
                Factory = factory,
                ResolvingType = type
            };

            return dependency;
        }

        string msg = string.Format(Messages.RegisteredTypeNotConcreteClass, type.FullName);
        throw new InvalidOperationException(msg);
    }

    /// <summary>
    /// Resolve the specified dependency type.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the dependency that is to be resolved.
    /// </typeparam>
    /// <returns>
    /// An instance of the resolving type that was bound to the dependency type.
    /// </returns>
    public T Resolve<T>() where T : class
    {
        Dependency<T>? dependency = (Dependency<T>?)GetDependency<T>();

        if (dependency is null)
        {
            string msg = string.Format(Messages.UnableToResolveUnknownDependency, typeof(T).FullName);
            throw new InvalidOperationException(msg);
        }

        Type resolvingType = dependency.ResolvingType;
        T resolvingObject;

        switch (dependency.Lifetime)
        {
            case DependencyLifetime.Singleton:
                lock (_lock)
                {
                    dependency.ResolvingObject ??= (T)Activator.CreateInstance(resolvingType)!;
                }

                resolvingObject = dependency.ResolvingObject;
                break;
            case DependencyLifetime.Transient:
                resolvingObject = (T)Activator.CreateInstance(resolvingType)!;
                break;
            default:
                string msg = string.Format(Messages.ResolvingScopedDependencyOutsideOfScope, typeof(T).FullName);
                throw new InvalidOperationException(msg);
        }

        return resolvingObject;
    }

    /// <summary>
    /// Add a dependency to the dependency injection container.
    /// </summary>
    /// <typeparam name="T">
    /// The type of dependency to be added.
    /// </typeparam>
    /// <param name="dependency">
    /// The dependency object containing the details of the dependency.
    /// </param>
    internal void AddDependency<T>(Dependency<T> dependency) where T : class
    {
        lock (_lock)
        {
            _dependencies[typeof(T)] = dependency;
        }
    }
}