using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BasicDI.Tests")]

namespace BasicDI;

/// <summary>
/// A simple dependency injection container.
/// </summary>
public class Container : IContainer
{
    /// <summary>
    /// The dependency mapping container.
    /// </summary>
    internal readonly Dictionary<Type, object> _dependencies = [];

    /// <summary>
    /// A list of dependency scopes that are currently active for this dependency injection
    /// container.
    /// </summary>
    internal readonly Dictionary<Guid, Scope> _scopes = [];

    /// <summary>
    /// A lazy initializer for the dependency injection container.
    /// </summary>
    private static readonly Lazy<Container> _lazy = new(static () => new Container());

    /// <summary>
    /// A lock object used to facilitate thread safety on operations against the dependency mapping
    /// container.
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// Save the <see cref="MethodInfo" /> details for the <see cref="Resolve{T}()" /> method so
    /// that we can create versions of the method for different generic types.
    /// </summary>
    private readonly MethodInfo _resolveMethodInfo
        = typeof(Container).GetMethod(nameof(Resolve), BindingFlags.Public | BindingFlags.Instance)!;

    /// <summary>
    /// Save the <see cref="MethodInfo" /> details for the <see cref="ResolveScoped{T}(IScope)" />
    /// method so that we can create versions of the method for different generic types.
    /// </summary>
    private readonly MethodInfo _resolveScopedMethodInfo
        = typeof(Container).GetMethod(nameof(ResolveScoped), BindingFlags.NonPublic | BindingFlags.Instance, [typeof(IScope)])!;

    /// <summary>
    /// The default constructor is marked private to prevent the user from invoking it directly.
    /// </summary>
    private Container()
    {
    }

    /// <summary>
    /// Get the current dependency injection container.
    /// </summary>
    /// <remarks>
    /// This returns a thread safe singleton instance of the container.
    /// </remarks>
    public static Container Current => _lazy.Value;

    /// <summary>
    /// Get a test instance of the dependency injection container for use in unit tests.
    /// </summary>
    /// <remarks>
    /// This returns a new instance of the container each time it is called.
    /// </remarks>
    internal static Container TestInstance => new();

    /// <summary>
    /// Create a new <see cref="Dependency{T}" /> object to be used for binding the specified
    /// dependency type and registering it with the dependency injection container.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the dependency object.
    /// </typeparam>
    /// <returns>
    /// A new <see cref="Dependency{T}" /> object representing the dependency.
    /// </returns>
    public ICanBindTo<T> Bind<T>() where T : class => new Dependency<T>(this);

    /// <summary>
    /// Create a new scope and add it to the scope list.
    /// </summary>
    /// <returns>
    /// An <see cref="IScope" /> object representing the scoped dependency lifetime.
    /// </returns>
    public IScope CreateScope()
    {
        Scope scope = new(this);

        lock (_lock)
        {
            _scopes[scope.Guid] = scope;
        }

        return scope;
    }

    /// <summary>
    /// Get the dependency object for the specified dependency type.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the dependency to get.
    /// </typeparam>
    /// <returns>
    /// The <see cref="IDependency{T}" /> object for the specified dependency type, or
    /// <see langword="null" /> if the dependency hasn't been registered in the container.
    /// </returns>
    public IDependency<T>? GetDependency<T>() where T : class
    {
        lock (_lock)
        {
            if (_dependencies.TryGetValue(typeof(T), out object? dependency))
            {
                return (IDependency<T>)dependency;
            }
        }

        return null;
    }

    /// <summary>
    /// Create a new <see cref="Dependency{T}" /> object to be used for registering the specified
    /// concrete dependency type with the dependency injection container.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the dependency object. Must be a concrete type. (not an interface)
    /// </typeparam>
    /// <param name="factory">
    /// Optional factory delegate for creating instances of the dependency type.
    /// </param>
    /// <returns>
    /// A new <see cref="Dependency{T}" /> object representing the dependency.
    /// </returns>
    /// <exception cref="DependencyInjectionException" />
    public ICanSpecifyLifetime Register<T>(Func<T>? factory = null) where T : class
    {
        Type type = typeof(T);

        if (type.IsClass && !type.IsAbstract)
        {
            Dependency<T> dependency = new(this)
            {
                Factory = factory,
                ResolvingType = type
            };

            return dependency;
        }

        string msg = string.Format(Messages.RegisteredTypeNotConcreteClass, type.FullName);
        throw new DependencyInjectionException(msg)
        {
            DependencyType = type,
            ResolvingType = type
        };
    }

    /// <summary>
    /// Resolve the specified dependency type.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the dependency that is to be resolved.
    /// </typeparam>
    /// <returns>
    /// An instance of the resolving type that was bound to the dependency type.
    /// </returns>
    /// <exception cref="DependencyInjectionException" />
    public T Resolve<T>() where T : class
    {
        Dependency<T> dependency = GetDependencyObject<T>();

        if (dependency.Lifetime is DependencyLifetime.Singleton && dependency.ResolvingObject is not null)
        {
            return dependency.ResolvingObject;
        }

        Type resolvingType = dependency.ResolvingType;
        ConstructorInfo constructorInfo = resolvingType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First();
        ParameterInfo[] parameterInfos = constructorInfo.GetParameters();
        object[] resolvingObjects = ResolveNestedDependencies(parameterInfos);

        return GetResolvingObject(dependency, constructorInfo, resolvingObjects);
    }

    /// <summary>
    /// Add a dependency to the dependency injection container.
    /// </summary>
    /// <typeparam name="T">
    /// The type of dependency to be added.
    /// </typeparam>
    /// <param name="dependency">
    /// The dependency object containing the details of the dependency.
    /// </param>
    internal void AddDependency<T>(Dependency<T> dependency) where T : class
    {
        lock (_lock)
        {
            _dependencies[typeof(T)] = dependency;
        }
    }

    /// <summary>
    /// Remove the specified scope from the list of active scopes.
    /// </summary>
    /// <param name="scope">
    /// The scope to be removed from the list of active scopes.
    /// </param>
    internal void RemoveScope(Scope scope)
    {
        lock (_lock)
        {
            _ = _scopes.Remove(scope.Guid);
        }
    }

    /// <summary>
    /// Resolve the given dependency type in the given scope.
    /// </summary>
    /// <typeparam name="T">
    /// The type of dependency that is to be resolved.
    /// </typeparam>
    /// <param name="scope">
    /// The effective scope that the dependency is being resolved in.
    /// </param>
    /// <returns>
    /// An instance of the resolving type that was bound to the dependency type.
    /// </returns>
    /// <exception cref="DependencyInjectionException" />
    internal T ResolveScoped<T>(IScope scope) where T : class
    {
        Scope activeScope = (Scope)scope;
        Dependency<T> dependency = GetDependencyObject<T>();

        if (dependency.Lifetime is DependencyLifetime.Scoped && activeScope.DependencyHasBeenResolved<T>())
        {
            return activeScope.GetResolvingObject<T>()!;
        }

        Type resolvingType = dependency.ResolvingType;
        ConstructorInfo constructorInfo = resolvingType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First();
        ParameterInfo[] parameterInfos = constructorInfo.GetParameters();
        object[] resolvingObjects = ResolveNestedDependencies(parameterInfos, activeScope);

        return GetResolvingObject(dependency, constructorInfo, resolvingObjects, activeScope);
    }

    /// <summary>
    /// Construct the resolving object from its <see cref="ConstructorInfo" /> and list of parameter
    /// values.
    /// </summary>
    /// <typeparam name="T">
    /// The dependency type that is being resolved.
    /// </typeparam>
    /// <param name="dependency">
    /// The <see cref="Dependency{T}" /> object describing the dependency.
    /// </param>
    /// <param name="constructorInfo">
    /// The <see cref="ConstructorInfo" /> for the resolving object.
    /// </param>
    /// <param name="parameterValues">
    /// The constructor parameter values.
    /// </param>
    /// <returns>
    /// An instance of the resolving object cast as the dependency type.
    /// </returns>
    /// <exception cref="DependencyInjectionException" />
    private T ConstructResolvingObject<T>(Dependency<T> dependency, ConstructorInfo constructorInfo, object[] parameterValues) where T : class
    {
        lock (_lock)
        {
            if (dependency.Factory is not null)
            {
                return dependency.Factory();
            }
        }

        try
        {
            return (T)constructorInfo.Invoke([.. parameterValues]);
        }
        catch (Exception ex)
        {
            string msg = string.Format(Messages.FailedToConstructResolvingObject, typeof(T).FullName, ex.Message);
            throw new DependencyInjectionException(msg, ex)
            {
                DependencyType = dependency.Type,
                Lifetime = dependency.Lifetime,
                ResolvingType = dependency.ResolvingType
            };
        }
    }

    /// <summary>
    /// Get the dependency object for the specified dependency type.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the dependency to get.
    /// </typeparam>
    /// <returns>
    /// The <see cref="Dependency{T}" /> object for the specified dependency type, or throws an
    /// exception if the dependency hasn't been registered in the container.
    /// </returns>
    /// <exception cref="DependencyInjectionException" />
    private Dependency<T> GetDependencyObject<T>() where T : class
    {
        lock (_lock)
        {
            if (_dependencies.TryGetValue(typeof(T), out object? dependency))
            {
                return (Dependency<T>)dependency;
            }
        }

        string msg = string.Format(Messages.UnableToResolveUnknownDependency, typeof(T).FullName);
        throw new DependencyInjectionException(msg)
        {
            DependencyType = typeof(T)
        };
    }

    /// <summary>
    /// Get an instance of the resolving object for the given dependency type.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the dependency that is being resolved.
    /// </typeparam>
    /// <param name="dependency">
    /// The <see cref="Dependency{T}" /> object describing the dependency.
    /// </param>
    /// <param name="constructorInfo">
    /// The <see cref="ConstructorInfo" /> for the resolving object.
    /// </param>
    /// <param name="parameterValues">
    /// A list of constructor parameter values.
    /// </param>
    /// <param name="scope">
    /// The dependency scope for the resolving object, or <see langword="null" /> if no scope is in
    /// effect.
    /// </param>
    /// <returns>
    /// An instance of the resolving object cast as the dependency type.
    /// </returns>
    /// <exception cref="DependencyInjectionException" />
    private T GetResolvingObject<T>(Dependency<T> dependency, ConstructorInfo constructorInfo, object[] parameterValues, Scope? scope = null) where T : class
    {
        return dependency.Lifetime switch
        {
            DependencyLifetime.Singleton => GetSingleton(dependency, constructorInfo, parameterValues),
            DependencyLifetime.Transient => GetTransient(dependency, constructorInfo, parameterValues),
            DependencyLifetime.Scoped => GetScoped(dependency, constructorInfo, parameterValues, scope),
            _ => throw new DependencyInjectionException(Messages.InvalidLifetime)
            {
                DependencyType = dependency.Type,
                Lifetime = dependency.Lifetime,
                ResolvingType = dependency.ResolvingType
            }
        };
    }

    /// <summary>
    /// Get the resolving object for a scoped dependency.
    /// </summary>
    /// <typeparam name="T">
    /// The dependency type being resolved.
    /// </typeparam>
    /// <param name="dependency">
    /// The <see cref="Dependency{T}" /> object describing the dependency.
    /// </param>
    /// <param name="constructorInfo">
    /// The <see cref="ConstructorInfo" /> for the resolving object.
    /// </param>
    /// <param name="parameterValues">
    /// A list of constructor parameter values.
    /// </param>
    /// <param name="scope">
    /// The dependency scope for the resolving object, or <see langword="null" /> if no scope is in
    /// effect.
    /// </param>
    /// <returns>
    /// An instance of the resolving object cast as the dependency type.
    /// </returns>
    /// <remarks>
    /// This method should be called only after verifying that the resolving object hasn't already
    /// been constructed.
    /// </remarks>
    /// <exception cref="DependencyInjectionException" />
    private T GetScoped<T>(Dependency<T> dependency, ConstructorInfo constructorInfo, object[] parameterValues, Scope? scope) where T : class
    {
        if (scope is null)
        {
            string msg = string.Format(Messages.ResolvingScopedDependencyOutsideOfScope, typeof(T).FullName);
            throw new DependencyInjectionException(msg)
            {
                DependencyType = dependency.Type,
                Lifetime = dependency.Lifetime,
                ResolvingType = dependency.ResolvingType
            };
        }
        else
        {
            T resolvingObject = ConstructResolvingObject(dependency, constructorInfo, parameterValues);
            scope.AddResolvingObject<T>(resolvingObject);
            return resolvingObject;
        }
    }

    /// <summary>
    /// Get the resolving object for a singleton dependency.
    /// </summary>
    /// <typeparam name="T">
    /// The dependency type being resolved.
    /// </typeparam>
    /// <param name="dependency">
    /// The <see cref="Dependency{T}" /> object describing the dependency.
    /// </param>
    /// <param name="constructorInfo">
    /// The <see cref="ConstructorInfo" /> for the resolving object.
    /// </param>
    /// <param name="parameterValues">
    /// A list of constructor parameter values.
    /// </param>
    /// <returns>
    /// An instance of the resolving object cast as the dependency type.
    /// </returns>
    /// <remarks>
    /// A new instance of the resolving object will be constructed only on the first time that this
    /// method is called. That same resolving object will be returned on all subsequent calls to
    /// this method.
    /// </remarks>
    /// <exception cref="DependencyInjectionException" />
    private T GetSingleton<T>(Dependency<T> dependency, ConstructorInfo constructorInfo, object[] parameterValues) where T : class
    {
        lock (_lock)
        {
            dependency.ResolvingObject ??= ConstructResolvingObject(dependency, constructorInfo, parameterValues);
            return dependency.ResolvingObject;
        }
    }

    /// <summary>
    /// Get the resolving object for a transient dependency.
    /// </summary>
    /// <typeparam name="T">
    /// The dependency type being resolved.
    /// </typeparam>
    /// <param name="dependency">
    /// The <see cref="Dependency{T}" /> object describing the dependency.
    /// </param>
    /// <param name="constructorInfo">
    /// The <see cref="ConstructorInfo" /> for the resolving object.
    /// </param>
    /// <param name="parameterValues">
    /// A list of constructor parameter values.
    /// </param>
    /// <returns>
    /// An instance of the resolving object cast as the dependency type.
    /// </returns>
    /// <remarks>
    /// A new instance of the resolving object will be constructed and returned on every call to
    /// this method.
    /// </remarks>
    /// <exception cref="DependencyInjectionException" />
    private T GetTransient<T>(Dependency<T> dependency, ConstructorInfo constructorInfo, object[] parameterValues) where T : class
            => ConstructResolvingObject(dependency, constructorInfo, parameterValues);

    /// <summary>
    /// Recursively resolve nested constructor dependencies found in the list of constructor
    /// parameter infos.
    /// </summary>
    /// <param name="parameterInfos">
    /// An array of <see cref="ParameterInfo" /> corresponding to the dependencies found in a single
    /// constructor method.
    /// </param>
    /// <param name="scope">
    /// The scope that the dependencies are being resolved in, or <see langword="null" /> if no
    /// scope is in effect.
    /// </param>
    /// <returns>
    /// An array of resolving objects corresponding to each of the dependencies found in the
    /// constructor.
    /// </returns>
    private object[] ResolveNestedDependencies(ParameterInfo[] parameterInfos, Scope? scope = null)
    {
        List<object> resolvingObjects = [];

        foreach (ParameterInfo parameterInfo in parameterInfos)
        {
            Type parameterType = parameterInfo.ParameterType;
            object? resolvingObject;

            if (scope is null)
            {
                MethodInfo resolveMethodInfo = _resolveMethodInfo.MakeGenericMethod(parameterType);
                resolvingObject = resolveMethodInfo.Invoke(this, []);
            }
            else
            {
                MethodInfo resolveScopedMethodInfo = _resolveScopedMethodInfo.MakeGenericMethod(parameterType);
                resolvingObject = resolveScopedMethodInfo.Invoke(this, [scope]);
            }

            resolvingObjects.Add(resolvingObject!);
        }

        return [.. resolvingObjects];
    }
}