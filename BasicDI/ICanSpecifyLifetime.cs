namespace BasicDI;

/// <summary>
/// Fluent interface that defines methods for specifying the dependency lifetime.
/// </summary>
public interface ICanSpecifyLifetime
{
    /// <summary>
    /// Add this <see cref="Dependency{T}" /> object to the dependency injection container as a
    /// scoped dependency.
    /// </summary>
    void AsScoped();

    /// <summary>
    /// Add this <see cref="Dependency{T}" /> object to the dependency injection container as a
    /// singleton dependency.
    /// </summary>
    void AsSingleton();

    /// <summary>
    /// Add this <see cref="Dependency{T}" /> object to the dependency injection container as a
    /// transient dependency.
    /// </summary>
    void AsTransient();
}