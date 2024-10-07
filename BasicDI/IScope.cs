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
}