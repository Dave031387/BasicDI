﻿namespace BasicDI;

/// <summary>
/// The exception that gets thrown when there are any issues with the dependency injection.
/// </summary>
[Serializable]
public class DependencyInjectionException : Exception
{
    /// <summary>
    /// Throw a <see cref="DependencyInjectionException" /> without specifying a message.
    /// </summary>
    public DependencyInjectionException()
    {
    }

    /// <summary>
    /// Throw a <see cref="DependencyInjectionException" /> with the specified message.
    /// </summary>
    /// <param name="message">
    /// The exception message that is to be displayed.
    /// </param>
    public DependencyInjectionException(string message) : base(message)
    {
    }

    /// <summary>
    /// Throw a <see cref="DependencyInjectionException" /> with the given message and inner
    /// exception.
    /// </summary>
    /// <param name="message">
    /// The exception message that is to be displayed.
    /// </param>
    /// <param name="inner">
    /// The inner exception.
    /// </param>
    public DependencyInjectionException(string message, Exception inner) : base(message, inner)
    {
    }
}