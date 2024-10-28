namespace BasicDI;

/// <summary>
/// A class used to manage a given dependency.
/// </summary>
/// <typeparam name="T">
/// The dependency type that is being managed.
/// </typeparam>
/// <param name="container">
/// A reference to the dependency injection container.
/// </param>
/// <remarks>
/// Class makes use of a primary constructor for creating new instances.
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
    /// Will return <see langword="null" /> if no factory is defined for this dependency.
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
        internal set;
    } = DependencyLifetime.Undefined;

    /// <summary>
    /// Gets an instance of the resolving object for this dependency.
    /// </summary>
    /// <remarks>
    /// Will return <see langword="null" /> if this dependency isn't a singleton or if the
    /// dependency hasn't been resolved yet.
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
    /// <exception cref="DependencyInjectionException" />
    /// "
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
            throw new DependencyInjectionException(msg1)
            {
                DependencyType = Type,
                ResolvingType = ResolvingType
            };
        }

        string msg2 = string.Format(Messages.IncompatibleResolvingType, ResolvingType.FullName, Type.FullName);
        throw new DependencyInjectionException(msg2)
        {
            DependencyType = Type,
            ResolvingType = ResolvingType
        };
    }
}