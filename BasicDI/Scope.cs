﻿namespace BasicDI;

/// <summary>
/// The <see cref="Scope" /> class is used manage the creation and lifetime of scoped dependencies.
/// </summary>
/// <param name="container">
/// A reference to the dependency injection container.
/// </param>
/// <remarks>
/// Class makes use of a primary constructor for creating new instances.
/// </remarks>
internal class Scope(Container container) : IScope
{
    /// <summary>
    /// Hold a reference to the dependency injection container.
    /// </summary>
    internal readonly Container _container = container;

    /// <summary>
    /// A dictionary of resolved dependency types within the current scope.
    /// </summary>
    internal readonly Dictionary<Type, object> _resolvingObjects = [];

    /// <summary>
    /// A lock object used to ensure thread safety.
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// Flag to detect redundant calls to the <see cref="Dispose(bool)" /> method.
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// Gets the <see cref="System.Guid" /> value that uniquely identifies this scope.
    /// </summary>
    public Guid Guid
    {
        get;
    } = Guid.NewGuid();

    /// <summary>
    /// Dispose of the managed resources that are owned by this scope.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Resolve the given scoped dependency type.
    /// </summary>
    /// <typeparam name="T">
    /// The dependency type that is to be resolved.
    /// </typeparam>
    /// <returns>
    /// The resolving object cast as the dependency type.
    /// </returns>
    public T Resolve<T>() where T : class
        => _container.ResolveScoped<T>(this);

    /// <summary>
    /// Add the resolving object for the given dependency type if no resolving object has yet been
    /// assigned to the dependency type.
    /// </summary>
    /// <typeparam name="T">
    /// The dependency type that the resolving object is to be assigned to.
    /// </typeparam>
    /// <param name="resolvingObject">
    /// The resolving object for the given dependency type.
    /// </param>
    internal void AddResolvingObject<T>(object resolvingObject) where T : class
    {
        Type dependencyType = typeof(T);

        lock (_lock)
        {
            if (!DependencyHasBeenResolved<T>())
            {
                _resolvingObjects[dependencyType] = resolvingObject;
            }
        }
    }

    /// <summary>
    /// Determine whether or not this scope contains the resolving object for the given dependency
    /// type.
    /// </summary>
    /// <typeparam name="T">
    /// The dependency type to be searched for.
    /// </typeparam>
    /// <returns>
    /// <see langword="true" /> if a resolving object has already been constructed for the given
    /// dependency type in this scope. Otherwise, returns <see langword="false" />.
    /// </returns>
    internal bool DependencyHasBeenResolved<T>() where T : class
        => _resolvingObjects.ContainsKey(typeof(T));

    /// <summary>
    /// Get the resolving object for the given dependency type.
    /// </summary>
    /// <typeparam name="T">
    /// The dependency type for which we want to retrieve the resolving object.
    /// </typeparam>
    /// <returns>
    /// The resolving object for the given dependency type, or <see langword="null" /> if no
    /// resolving object is found.
    /// </returns>
    internal T? GetResolvingObject<T>() where T : class
    {
        lock (_lock)
        {
            if (DependencyHasBeenResolved<T>())
            {
                return (T)_resolvingObjects[typeof(T)];
            }
        }

        return null;
    }

    /// <summary>
    /// Dispose of the managed resources that are owned by this scope and then set a flag to prevent
    /// redundant calls to this method.
    /// </summary>
    /// <param name="disposing">
    /// A boolean flag indicating whether or not managed resources should be disposed of.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposing)
        {
            _container.RemoveScope(this);
            _resolvingObjects.Clear();
        }

        // Unmanaged resources would be freed here if there were any.

        _isDisposed = true;
    }
}