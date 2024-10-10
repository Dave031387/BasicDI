namespace BasicDI;

using System;

/// <summary>
/// An interface that defines an object for managing scoped lifetime in a dependency injection
/// container.
/// </summary>
public interface IScope : IDisposable
{
    /// <summary>
    /// Gets the <see cref="System.Guid" /> value that uniquely identifies this scope.
    /// </summary>
    Guid Guid
    {
        get;
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
    T Resolve<T>() where T : class;
}