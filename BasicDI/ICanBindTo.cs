namespace BasicDI;

/// <summary>
/// Fluent interface that defines methods for binding the resolving type to the dependency type.
/// </summary>
/// <typeparam name="T">
/// The dependency type that is being bound to the resolving type.
/// </typeparam>
public interface ICanBindTo<T> where T : class
{
    /// <summary>
    /// Bind the resolving type to the dependency type.
    /// </summary>
    /// <typeparam name="TResolving">
    /// The type of the resolving object.
    /// </typeparam>
    /// <param name="factory">
    /// Optional factory delegate for creating instances of the resolving type.
    /// </param>
    /// <returns>
    /// This updated <see cref="Dependency{T}" /> instance.
    /// </returns>
    /// <exception cref="DependencyInjectionException" />"
    ICanSpecifyLifetime To<TResolving>(Func<T>? factory = null) where TResolving : class;
}