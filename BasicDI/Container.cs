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
    private static readonly Lazy<IContainer> _lazy = new(() => new Container());

    /// <summary>
    /// A lock object used to facilitate thread safety on operations against the dependency
    /// injection container.
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// Save the <see cref="MethodInfo" /> details for the <see cref="Resolve{T}()" /> method so
    /// that we can dynamically invoke the method for different generic types.
    /// </summary>
    private readonly MethodInfo _resolveMethodInfo
        = typeof(Container).GetMethod(nameof(Resolve), BindingFlags.Public | BindingFlags.Instance)!;

    /// <summary>
    /// Save the <see cref="MethodInfo" /> details for the <see cref="ResolveScoped{T}(IScope)" />
    /// method so that we can dynamically invoke the method for different generic types.
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
    public static IContainer Current => _lazy.Value;

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
    /// The dependency type. Must be a concrete type or an interface type.
    /// </typeparam>
    /// <returns>
    /// A new <see cref="Dependency{T}" /> instance cast as a <see cref="ICanBindTo{T}" /> object.
    /// </returns>
    /// <exception cref="DependencyInjectionException" />
    public ICanBindTo<T> Bind<T>() where T : class
    {
        Type dependencyType = typeof(T);

        if (dependencyType.IsInterface || (dependencyType.IsClass && !dependencyType.IsAbstract))
        {
            return new Dependency<T>(this);
        }

        string msg = string.Format(Messages.DependencyTypeNotValid, dependencyType.FullName);
        throw new DependencyInjectionException(msg)
        {
            DependencyType = dependencyType
        };
    }

    /// <summary>
    /// Create a new scope and add it to the scope list.
    /// </summary>
    /// <returns>
    /// An <see cref="IScope" /> object for managing the scoped dependency lifetime.
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
    /// Get the <see cref="IDependency{T}" /> instance for the specified dependency type.
    /// </summary>
    /// <typeparam name="T">
    /// The dependency type to look for.
    /// </typeparam>
    /// <returns>
    /// The <see cref="IDependency{T}" /> instance for the specified dependency type, or
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
    /// Create a new <see cref="Dependency{T}" /> instance to be used for registering the specified
    /// dependency type with the dependency injection container.
    /// </summary>
    /// <typeparam name="T">
    /// The dependency type to register. Must be a concrete type if <paramref name="factory" /> is
    /// <see langword="null" />. Can be an interface type if <paramref name="factory" /> is not
    /// <see langword="null" />.
    /// </typeparam>
    /// <param name="factory">
    /// Optional factory delegate for creating instances of the dependency type.
    /// </param>
    /// <returns>
    /// A new <see cref="Dependency{T}" /> instance cast as an <see cref="ICanSpecifyLifetime" />
    /// object.
    /// </returns>
    /// <exception cref="DependencyInjectionException" />
    public ICanSpecifyLifetime Register<T>(Func<T>? factory = null) where T : class
    {
        Type type = typeof(T);

        if ((type.IsClass && !type.IsAbstract) ||
            (type.IsInterface && factory is not null))
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
    /// Get the resolving instance for the specified dependency type.
    /// </summary>
    /// <typeparam name="T">
    /// The dependency type that is to be resolved.
    /// </typeparam>
    /// <returns>
    /// An instance of the resolving type that was bound to the dependency type.
    /// </returns>
    /// <remarks>
    /// This method is invoked recursively until all nested dependencies of the given dependency
    /// type have been resolved.
    /// </remarks>
    /// <exception cref="DependencyInjectionException" />
    public T Resolve<T>() where T : class
    {
        Dependency<T> dependency = GetDependencyObject<T>();

        if (DependencyHasBeenResolved(dependency))
        {
            return GetResolvedDependency(dependency)!;
        }

        ConstructorInfo? constructorInfo = null;
        object[] resolvingObjects = [];

        if (dependency.Factory is null)
        {
            constructorInfo = GetConstructorInfo(dependency.ResolvingType);
            resolvingObjects = ResolveNestedDependencies(constructorInfo);
        }

        return GetResolvingInstance(dependency, constructorInfo, resolvingObjects);
    }

    /// <summary>
    /// Add a dependency to the dependency injection container.
    /// </summary>
    /// <typeparam name="T">
    /// The dependency type to be added.
    /// </typeparam>
    /// <param name="dependency">
    /// The <see cref="Dependency{T}" /> object containing the details of the given dependency type.
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
    /// The dependency type that is to be resolved.
    /// </typeparam>
    /// <param name="scope">
    /// The effective scope that the dependency is being resolved in.
    /// </param>
    /// <returns>
    /// An instance of the resolving type that was bound to the dependency type.
    /// </returns>
    /// <remarks>
    /// This method is invoked recursively until all nested dependencies of the given dependency
    /// type have been resolved.
    /// </remarks>
    /// <exception cref="DependencyInjectionException" />
    internal T ResolveScoped<T>(IScope scope) where T : class
    {
        Scope activeScope = (Scope)scope;
        Dependency<T> dependency = GetDependencyObject<T>();

        if (DependencyHasBeenResolved(dependency, activeScope))
        {
            return GetResolvedDependency(dependency, activeScope)!;
        }

        ConstructorInfo constructorInfo = GetConstructorInfo(dependency.ResolvingType);
        object[] resolvingObjects = ResolveNestedDependencies(constructorInfo, activeScope);

        return GetResolvingInstance(dependency, constructorInfo, resolvingObjects, activeScope);
    }

    /// <summary>
    /// Determine if the given dependency has already been resolved.
    /// </summary>
    /// <typeparam name="T">
    /// The dependency type to be checked.
    /// </typeparam>
    /// <param name="dependency">
    /// The <see cref="Dependency{T}" /> object describing the dependency.
    /// </param>
    /// <param name="scope">
    /// An optional scope that the dependency is being resolved in. Defaults to
    /// <see langword="null" /> if no scope is in effect.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if a resolving object exists for the given dependency type,
    /// otherwise <see langword="false" /> if no resolving object exists.
    /// </returns>
    private static bool DependencyHasBeenResolved<T>(Dependency<T> dependency, Scope? scope = null) where T : class
    {
        if (IsSingleton(dependency))
        {
            return dependency.ResolvingObject is not null;
        }
        else if (IsScoped(dependency, scope))
        {
            return scope!.DependencyHasBeenResolved<T>();
        }

        return false;
    }

    /// <summary>
    /// Get the <see cref="ConstructorInfo" /> for the given class type.
    /// </summary>
    /// <param name="type">
    /// The class type for which we want to retrieve the constructor info.
    /// </param>
    /// <returns>
    /// The <see cref="ConstructorInfo" /> object for the given class type.
    /// </returns>
    /// <remarks>
    /// If there is more than one constructor for the given class type, then the info for the
    /// constructor having the most parameters will be returned.
    /// </remarks>
    /// <exception cref="DependencyInjectionException" />
    private static ConstructorInfo GetConstructorInfo(Type type)
    {
        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        ConstructorInfo[] constructorInfos = type.GetConstructors(bindingFlags);

        if (constructorInfos.Length < 1)
        {
            string msg = string.Format(Messages.NoConstructorsFound, type.FullName);
            throw new DependencyInjectionException(msg)
            {
                ResolvingType = type
            };
        }

        int maxParameterCount = -1;
        int constructorIndex = -1;

        for (int i = 0; i < constructorInfos.Length; i++)
        {
            int parameterCount = constructorInfos[i].GetParameters().Length;

            if (parameterCount > maxParameterCount)
            {
                maxParameterCount = parameterCount;
                constructorIndex = i;
            }
        }

        return constructorInfos[constructorIndex];
    }

    /// <summary>
    /// Get the resolving object instance for the given dependency type.
    /// </summary>
    /// <typeparam name="T">
    /// The dependency type being resolved.
    /// </typeparam>
    /// <param name="dependency">
    /// The <see cref="Dependency{T}" /> object describing the dependency.
    /// </param>
    /// <param name="scope">
    /// An optional scope that the dependency is being resolved in. Defaults to
    /// <see langword="null" /> if no scope is in effect.
    /// </param>
    /// <returns>
    /// An instance of the resolving object, or <see langword="null" /> if no resolving object was
    /// found.
    /// </returns>
    private static T? GetResolvedDependency<T>(Dependency<T> dependency, Scope? scope = null) where T : class
    {
        if (IsSingleton(dependency))
        {
            return dependency.ResolvingObject;
        }

        // IMPORTANT: At this point we must be working with a scoped object in an active scope. Care
        // must be taken if the code is refactored to ensure that this is always the case.
        return scope!.GetResolvingObject<T>();
    }

    /// <summary>
    /// Determine if the given dependency type is a scoped dependency in an active scope.
    /// </summary>
    /// <typeparam name="T">
    /// The dependency type to be checked.
    /// </typeparam>
    /// <param name="dependency">
    /// The <see cref="Dependency{T}" /> that describes the dependency.
    /// </param>
    /// <param name="scope">
    /// The scope of the dependency.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="scope" /> isn't <see langword="null" /> and the
    /// given dependency is a scoped dependency. Otherwise, returns <see langword="false" />.
    /// </returns>
    private static bool IsScoped<T>(Dependency<T> dependency, Scope? scope) where T : class
        => dependency.Lifetime is DependencyLifetime.Scoped && scope is not null;

    /// <summary>
    /// Determine if the given dependency is a singleton dependency.
    /// </summary>
    /// <typeparam name="T">
    /// The dependency type to be checked.
    /// </typeparam>
    /// <param name="dependency">
    /// The <see cref="Dependency{T}" /> that describes the dependency.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if the given dependency is a singleton dependency. Otherwise,
    /// returns <see langword="false" />.
    /// </returns>
    private static bool IsSingleton<T>(Dependency<T> dependency) where T : class
        => dependency.Lifetime is DependencyLifetime.Singleton;

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
    private T ConstructResolvingObject<T>(Dependency<T> dependency,
                                          ConstructorInfo? constructorInfo,
                                          object[] parameterValues) where T : class
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
            return (T)constructorInfo!.Invoke([.. parameterValues]);
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
    /// Get the <see cref="Dependency{T}" /> object for the specified dependency type.
    /// </summary>
    /// <typeparam name="T">
    /// The dependency type to get.
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
    /// The dependency type that is being resolved.
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
    private T GetResolvingInstance<T>(Dependency<T> dependency,
                                      ConstructorInfo? constructorInfo,
                                      object[] parameterValues,
                                      Scope? scope = null) where T : class
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
    /// been constructed for the given scope.
    /// </remarks>
    /// <exception cref="DependencyInjectionException" />
    private T GetScoped<T>(Dependency<T> dependency,
                           ConstructorInfo? constructorInfo,
                           object[] parameterValues,
                           Scope? scope) where T : class
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
    /// This method should be called only after verifying that the resolving object hasn't already
    /// been constructed.
    /// </remarks>
    /// <exception cref="DependencyInjectionException" />
    private T GetSingleton<T>(Dependency<T> dependency,
                              ConstructorInfo? constructorInfo,
                              object[] parameterValues) where T : class
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
    private T GetTransient<T>(Dependency<T> dependency,
                              ConstructorInfo? constructorInfo,
                              object[] parameterValues) where T : class
            => ConstructResolvingObject(dependency, constructorInfo, parameterValues);

    /// <summary>
    /// Recursively resolve nested dependencies found in the constructor info.
    /// </summary>
    /// <param name="constructorInfo">
    /// The <see cref="ConstructorInfo" /> for the resolving object.
    /// </param>
    /// <param name="scope">
    /// The scope that the dependencies are being resolved in, or <see langword="null" /> if no
    /// scope is in effect.
    /// </param>
    /// <returns>
    /// An array of resolving objects corresponding to each of the dependencies found in the
    /// constructor.
    /// </returns>
    /// <exception cref="DependencyInjectionException" />
    private object[] ResolveNestedDependencies(ConstructorInfo constructorInfo, Scope? scope = null)
    {
        ParameterInfo[] parameterInfos = constructorInfo.GetParameters();
        List<object> resolvingObjects = [];

        foreach (ParameterInfo parameterInfo in parameterInfos)
        {
            Type parameterType = parameterInfo.ParameterType;
            object? resolvingObject = ResolveNestedDependency(parameterType, scope);

            if (resolvingObject is null)
            {
                // TODO: This block has not been unit tested. Should be an impossible condition.
                string msg = string.Format(Messages.ResolvingObjectIsNull, parameterType.FullName);
                throw new DependencyInjectionException(msg)
                {
                    DependencyType = parameterType
                };
            }

            resolvingObjects.Add(resolvingObject);
        }

        return [.. resolvingObjects];
    }

    /// <summary>
    /// This method makes a recursive call back to either the <see cref="Resolve{T}" /> or
    /// <see cref="ResolveScoped{T}(IScope)" /> method to resolve the given nested dependency type.
    /// </summary>
    /// <param name="type">
    /// The nested dependency type to be resolved.
    /// </param>
    /// <param name="scope">
    /// The scope that the dependency is being resolved in, or <see langword="null" /> if no scope
    /// is in effect.
    /// </param>
    /// <returns>
    /// An instance of the resolving object for the given nested dependency type passed as an
    /// <see langword="object" /> reference.
    /// </returns>
    private object? ResolveNestedDependency(Type type, Scope? scope = null)
    {
        if (scope is null)
        {
            MethodInfo resolveMethodInfo = _resolveMethodInfo.MakeGenericMethod(type);
            return resolveMethodInfo.Invoke(this, []);
        }
        else
        {
            MethodInfo resolveScopedMethodInfo = _resolveScopedMethodInfo.MakeGenericMethod(type);
            return resolveScopedMethodInfo.Invoke(this, [scope]);
        }
    }
}