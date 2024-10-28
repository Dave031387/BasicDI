namespace BasicDI;

/// <summary>
/// The exception that gets thrown when there are any issues with the dependency injection.
/// </summary>
[Serializable]
public class DependencyInjectionException : Exception
{
    /// <summary>
    /// Throw a <see cref="DependencyInjectionException" /> with the specified message.
    /// </summary>
    /// <param name="message">
    /// The exception message that is to be displayed.
    /// </param>
    internal DependencyInjectionException(string message) : base(message)
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
    internal DependencyInjectionException(string message, Exception inner) : base(message, inner)
    {
    }

    /// <summary>
    /// The dependency type.
    /// </summary>
    public Type? DependencyType
    {
        get; internal set;
    }

    /// <summary>
    /// The dependency lifetime.
    /// </summary>
    public DependencyLifetime Lifetime
    {
        get; internal set;
    }

    /// <summary>
    /// The resolving type.
    /// </summary>
    public Type? ResolvingType
    {
        get; internal set;
    }
}