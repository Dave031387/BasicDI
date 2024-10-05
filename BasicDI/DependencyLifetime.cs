namespace BasicDI;

/// <summary>
/// An enumeration of valid dependency lifetime values.
/// </summary>
public enum DependencyLifetime
{
    /// <summary>
    /// The dependency lifetime is undefined.
    /// </summary>
    Undefined,

    /// <summary>
    /// The dependency lifetime is singleton.
    /// </summary>
    Singleton,

    /// <summary>
    /// The dependency lifetime is scoped.
    /// </summary>
    Scoped,

    /// <summary>
    /// The dependency lifetime is transient.
    /// </summary>
    Transient
}