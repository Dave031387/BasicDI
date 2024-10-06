namespace BasicDI;

/// <summary>
/// The <see cref="Dependency{T}" /> class defines the mapping of a dependency type to its resolving
/// type.
/// </summary>
/// <typeparam name="T">
/// The type of the dependency object that this <see cref="Dependency{T}" /> class instance
/// describes.
/// </typeparam>
/// <param name="container">
/// A reference to the dependency injection container.
/// </param>
/// <remarks>
/// Creates a new instance of the <see cref="Dependency{T}" /> class.
/// </remarks>
internal class Dependency<T>(Container container) : IDependency<T>, ICanBindTo<T>, ICanSpecifyLifetime where T : class
{
    /// <summary>
    /// A reference to the dependency injection container.
    /// </summary>
    internal readonly Container _container = container;

    /// <summary>
    /// Gets the factory object used for creating instances of the resolving type.
    /// </summary>
    /// <remarks>
    /// <see langword="null" /> will be returned if no factory is defined for this dependency.
    /// </remarks>
    public Func<T>? Factory
    {
        get;
        internal set;
    }

    /// <summary>
    /// Gets the lifetime of the dependency.
    /// </summary>
    /// <remarks>
    /// <see cref="DependencyLifetime.Undefined" /> will be returned if no lifetime has been set for
    /// this dependency.
    /// </remarks>
    public DependencyLifetime Lifetime
    {
        get;
        private set;
    } = DependencyLifetime.Undefined;

    /// <summary>
    /// Gets an instance of the resolving object for this dependency.
    /// </summary>
    /// <remarks>
    /// <see langword="null" /> will be returned if this dependency isn't a singleton.
    /// </remarks>
    public T? ResolvingObject
    {
        get;
        internal set;
    }

    /// <summary>
    /// Gets the type of the resolving object for this dependency.
    /// </summary>
    /// <remarks>
    /// This property will return <see langword="typeof" />( <see cref="object" />) if no resolving
    /// type has been specified.
    /// </remarks>
    public Type ResolvingType
    {
        get;
        internal set;
    } = typeof(object);

    /// <summary>
    /// Gets the type of this dependency.
    /// </summary>
    public Type Type => typeof(T);

    /// <summary>
    /// Add this <see cref="Dependency{T}" /> object to the dependency injection container as a
    /// scoped dependency.
    /// </summary>
    public void AsScoped()
    {
        Lifetime = DependencyLifetime.Scoped;
        _container.AddDependency(this);
    }

    /// <summary>
    /// Add this <see cref="Dependency{T}" /> object to the dependency injection container as a
    /// singleton dependency.
    /// </summary>
    public void AsSingleton()
    {
        Lifetime = DependencyLifetime.Singleton;
        _container.AddDependency(this);
    }

    /// <summary>
    /// Add this <see cref="Dependency{T}" /> object to the dependency injection container as a
    /// transient dependency.
    /// </summary>
    public void AsTransient()
    {
        Lifetime = DependencyLifetime.Transient;
        _container.AddDependency(this);
    }

    /// <summary>
    /// Assign the resolving type to the dependency.
    /// </summary>
    /// <typeparam name="TResolving">
    /// The type of the resolving object.
    /// </typeparam>
    /// <param name="factory">
    /// Optional factory delegate for creating instances of the resolving type.
    /// </param>
    /// <returns>
    /// This updated <see cref="Dependency{T}" /> object.
    /// </returns>
    public ICanSpecifyLifetime To<TResolving>(Func<T>? factory = null) where TResolving : class
    {
        ResolvingType = typeof(TResolving);

        if (ResolvingType.IsAssignableTo(typeof(T)))
        {
            if (ResolvingType.IsClass && !ResolvingType.IsAbstract)
            {
                Factory = factory;
                return this;
            }

            string msg1 = string.Format(Messages.ResolvingTypeNotConcreteClass, ResolvingType.FullName, Type.FullName);
            throw new InvalidOperationException(msg1);
        }

        string msg2 = string.Format(Messages.IncompatibleResolvingType, ResolvingType.FullName, Type.FullName);
        throw new InvalidOperationException(msg2);
    }
}