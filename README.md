# *BasicDI* Class Library
## Overview
The ***BasicDI*** class library was built as part of a training exercise to learn more about dependency injection and how it can be implemented.
The library contains the basic functionality to handle constructor dependency injection. The key features include:

- Support for transient, singleton, and scoped dependencies
- Support of factory methods for creating dependency objects
- Basic support for generic dependencies
- Fluent interface for registering dependencies with the dependency injection container
- Logic for handling dependencies that have more than one constructor
- Comprehensive error checking and use of the ***DependencyInjectionException*** class for reporting all exceptions
- Thread safety (to the best of my knowledge)

Although this is a bare-bones implementation of dependency injection, it should still be suitable for use in most small to moderate sized projects.

## The Components of the BasicDI Class Library
There are three main components making up this class library:

- The ***IDependency\<T>*** object is used to represent a single dependency. The type parameter ***T*** is the dependency type being represented.
  Each unique dependency type is represented by a single ***IDependency\<T>*** object.
- The ***IScope*** object is used to represent a single dependency scope. It keeps track of all scoped dependencies that have been resolved within
  the dependency scope.
- The ***IContainer*** object is the dependency injection container. It manages the registering and resolving of all dependencies, as well as creating
  and keeping track of all active dependency scopes.

## Dependency Lifetime
Dependencies that are managed by the ***BasicDI*** class library have a defined lifetime. There are three available options:

- ***Transient*** - Whenever you want a new instance of a dependency every time it is asked for, you assign the ***Transient*** lifetime to
  that dependency. There can be many instances of a dependency having this lifetime. Each instance will be disposed of when the application
  reaches the end of the code block in which it was created.
- ***Singleton*** - Whenever you want the same instance of a dependency every time it is asked for, you assign the ***Singleton*** lifetime to
  that dependency. There will always be only one instance created for a dependency having this lifetime. The instance won't be disposed of until
  the end of the application.
- ***Scoped*** - Dependencies having a ***Scoped*** lifetime will have only one instance of the dependency created in each scope that asks
  for the dependency. If you ask for the same scoped dependency more than once within a scope, the same instance will be returned each time.
  The instance will be disposed of when the end of the block is reached where the scope was created.

## Setting up the Dependency Injection Container
In order to work properly, the dependency injection container needs to be created at the top-most level of your project. For example, in a
console application, you would create the container in the ***Program.cs*** file. In a Windows WPF application, the ***App.xaml.cs*** file is the
appropriate place for creating the container. The reason for this is that all dependencies must be registered with the container before you can
instantiate any class having dependencies.

Creating and using the dependency injection container generally follows this pattern:

1. Create an instance of the dependency injection container
1. Register all dependencies with the container
1. Ask the container for an instance of the main class used to kick off your application
1. Call the appropriate method on the main class to start your application

> [!Note]
> *The ***BasicDI*** dependency injection container will resolve dependencies recursively. So, when you ask the container for an instance of the
> main class used for kicking off the application, not only will the immediate dependencies in that class's constructor get resolved, but all
> dependencies in the constructors of its dependencies will be resolved, and so on until every dependency in every class constructor is resolved.*

## Creating an Instance of the Dependency InjectionContainer
The static ***Container.Current*** property is used to create the instance of the dependency injection container.

*Example:*
```csharp
private IContainer container = Container.Current;
```

The ***Container.Current*** property will always return the same dependency injection container instance wherever it is called. In other words,
there can be only one dependency injection container in any given project that references the ***BasicDI*** class library.

> [!Note]
> *Any assembly in your project that needs direct access to the ***IContainer***, ***IDependency***, or ***IScope*** objects of the dependency
> injection container must have a reference to the ***BasicDI*** class library.*

## Registering Dependencies with the Container
Before dependencies can be injected into class constructors, they must first be registered with the dependency injection container so that
the container knows how to handle them. A fluent API is used for registering dependencies. The following example shows how you would
register dependencies with the container.

*Example:*
```csharp
private IContainer container = Container.Current;
container.Bind<IPerson>().To<Employee>().AsTransient();
container.Bind<ILogger>().To<FileLogger>().AsSingleton();
container.Bind<IDBObject>().To<DBObject>().AsScoped();
```

The ***Bind\<T>()*** method creates a new ***IDependency\<T>*** object. The type parameter ***T*** must be an interface or class type and must not be
an abstract class.

The ***To\<T>()*** method adds the resolving type to the ***IDependency\<T>*** object that was created by the ***Bind\<T>()*** method. The type
parameter ***T*** must be a concrete class type and must not be an abstract class. If the type parameter on the ***Bind\<T>()*** method is an
interface, then the type specified on the ***To\<T>()*** method must be a class that implements that interface. If the type parameter on the
***Bind\<T>()*** method is a class type, then the type specified on the ***To\<T>()*** method must be a class type that derives from the class
type on the ***Bind\<T>()*** method. (Or it can be the same type as was specified on the ***Bind\<T>()*** method.)

The ***AsTransient()***, ***AsSingleton()***, and ***AsScoped()*** methods set the dependency lifetime for the ***IDependency\<T>*** object that was
created by the ***Bind\<T>()*** method. These methods also add the ***IDependency\<T>*** object to the dependency injection container.

Sometimes you may want to register a concrete class with the dependency injection container without binding it to any other resolving type. You can
accomplish this with the ***Register\<T>()*** method as shown in the next example.

*Example:*
```csharp
private IContainer container = Container.Current;
container.Register<MainWindow>().AsSingleton();
```

The above example is equivalent to the following:

```csharp
private IContainer container = Container.Current;
container.Bind<MainWindow>().To<MainWindow>().AsSingleton();
```

Normally you would do something like this if the class in question (***MainWindow*** in this case) has constructor dependencies but no suitable
interface for it to be bound to. Registering such a class (or binding it to itself) allows you to ask for an instance of the class having all
dependencies resolved by the dependency injection container.

> [!Warning]
> *If you don't call one of the **As**\[Lifetime]**()** methods after the ***To\<T>()*** or ***Register\<T>()*** methods, then the dependency will not
> get added to the dependency injection container, and you will not be able to request instances of the dependency from the container.*

> [!Warning]
> *If you bind or register the same dependency type more than once, the last ***Bind\<T>()*** or ***Register\<T>()*** method that is executed
> will be the effective one. No errors, warnings, or exceptions will be given.*

## Specifying a Factory Method
The ***BasicDI*** class library allows you to specify a factory method to be used for creating new instances of the resolving type. To do this
you simply provide a method that returns either an object of the dependency type, or an object whose type is assignable to the dependency
type. This method is then passed as an optional parameter to the ***To\<T>()*** or ***Register\<T>()*** method.

*Example:*
```csharp
static Employee EmployeeFactory()
{
    return new Employee()
    {
        ID = Guid.NewGuid(),
        CreatedOn = DateTime.Now
    };
}

private IContainer container = Container.Current;
container.Bind<IPerson>().To<Employee>(EmployeeFactory).AsTransient();
```

In the above example, whenever a new instance of the ***IPerson*** dependency is asked for, the ***EmployeeFactory()*** method will be called to
create the instance. This example assumes the ***Employee*** class has a parameterless constructor. If the ***Employee*** class happened to have
any dependencies in its constructor, then the ***EmployeeFactory()*** method would need to retrieve those dependencies from the dependency
injection container prior to creating the new instance of the ***Employee*** class, passing those dependencies to the constructor. Refer to the
next section in this document regarding resolving dependencies.

> [!Note]
> *Factory methods are required only when some type of setup needs to be performed on the resolving object prior to that object being passed back
> to the caller of the ***Resolve\<T>()*** method. In the example above, the ***ID*** and ***CreatedOn*** properties of the Employee object are
> set to appropriate values by the factory method. If the factory method hadn't been used, the ***Employee*** object would have been returned
> with the ***ID*** and ***CreatedOn*** properties set to default values.*

## Resolving Dependencies
After all dependencies have been registered with the dependency injection container, you can begin to ask for dependency objects from the container.
The ***Resolve\<T>()*** method is used to request the resolving object corresponding to a given dependency type. The type parameter ***T*** specifies
the dependency type being resolved.

*Example:*
```csharp
private IContainer container = Container.Current;
container.Bind<ILogger>().To<ConsoleLogger>().AsSingleton();
private ILogger logger = container.Resolve<ILogger>();
```

The last line in the example above retrieves an instance of the ***ConsoleLogger*** class and assigns it to the ***logger*** variable.

The actions performed by the ***Resolve\<T>()*** method of the ***Container*** class depends on the lifetime of the dependency being resolved
and the current state of the corresponding ***IDependency\<T>*** object.

- For dependencies having a ***Transient*** lifetime, a new instance of the resolving type is created and returned each time the
  ***Resolve\<T>()*** method is called for that dependency type.
- For dependencies having a ***Singleton*** lifetime:
    - The first time ***Resolve\<T>()*** is called for the dependency type, a new instance of the resolving type is created and returned to
      the caller. Before returning the instance, though, a reference to the object is saved in the corresponding ***IDependency\<T>*** object
      held by the ***IContainer*** object.
    - Each time after the first time ***Resolve\<T>()*** is called for the same dependency type, the reference that was saved in the
      corresponding ***IDependency\<T>*** object is returned to the caller.
- Calling the ***Resolve\<T>()*** method of the ***Container*** class for a dependency having a ***Scoped*** lifetime will result in an
  exception being thrown. Instead, you should use the ***Resolve\<T>()*** method of the appropriate ***IScope*** object to resolve scoped
  dependencies. (see next topic)

If the ***IDependency\<T>*** object specifies a factory method, then the factory method will be used by the ***Resolve\<T>()*** method
for creating the resolving class instance. Otherwise, the ***Resolve\<T>()*** method will handle the creation of the resolving instance.

When a dependency is resolved, if that dependency itself has one or more dependencies in its constructor, then the ***Resolve\<T>()*** method
will be called recursively for each of those dependencies until all inter-related dependencies have been resolved. Only then can the method
return the correctly constructed instance of the requested resolving type.

> [!Caution]
> *Normally, in order for the dependency injection process to work as expected, each resolving class object should have only one constructor
> and that constructor should have parameters for each dependency that is needed for that class. The ***BasicDI*** class library, though,
> allows a resolving class to have more than one constructor. In that situation, the class library will use the constructor having the
> most parameters as the one to use in resolving dependencies (unless a factory method is specified). Care must be taken by the programmer
> to ensure that the constructor having the most parameters is the correct one to use.*

## Scoped Dependencies
As the name implies, scoped dependencies are dependencies that are resolved within a given scope. Each active scope keeps track of its own
copies of its resolved scoped dependencies.

Within a scope, a scoped dependency acts similar to a singleton dependency. The first time a particular scoped dependency type is resolved in
the scope, a new instance of the resolving object is created and returned. A reference to the resolving object is also saved in the corresponding
***IScope*** object which is held by the ***IContainer*** object. Each additional time that particular dependency type is resolved in a given
scope, the saved reference is returned to the caller.

If two scopes are active at the same time and a given scoped dependency type is requested in each scope, a new instance of the resolving object
will be created in each scope.

Before you can resolve scoped dependencies you must first create the dependency scope. This is done by calling the ***CreateScope()*** method
on the ***Container*** class.

*Example:*
```csharp
private IContainer container = Container.Current;

using (IScope scope = container.CreateScope())
{
    // Statements for processing scoped dependencies go here
}
```

Since the ***IScope*** object implements the ***IDisposable*** interface, you would normally create the scope in a *using* block as shown
above. Statements for resolving scoped dependencies and processing scoped dependencies should be contained within the *using* block. The
***Resolve\<T>()*** method of the ***Scope*** class is used for resolving scoped dependencies.

```csharp
private IContainer container = Container.Current;
container.Bind<ITransaction>().To<BillingTransaction>().AsScoped();

using (IScope scope = container.CreateScope())
{
    ITransaction transaction = scope.Resolve<ITransaction>();
    // Statements for processing scoped dependencies go here
}
```

Scoped dependencies may themselves have dependencies on transient or singleton dependency types. These transient and singleton dependencies
don't belong to any scope and will be resolved in the same way as described in the previous topic.

> [!Note]
> *Although not generally useful, it is permissible to embed one dependency scope within another.*

# *BasicDI* User API
## Overview
The ***BasicDI*** class library user interface is made up of three primary interfaces.

1. The ***IDependency\<T>*** interface describes a single dependency type and its resolving type.
1. The ***IScope*** interface represents a single dependency scope.
1. The ***IContainer*** interface provides access to the dependency injection container.

In addition to these three interfaces, there are two interfaces on the ***Dependency\<T>*** object that support the fluent interface for
binding and registering dependencies with the dependency injection container.

1. The ***ICanBindTo\<T>*** interface provides the ***To\<TResolving>()*** method.
1. The ***ICanSpecifyLifetime*** interface provides the ***AsScoped()***, ***AsSingleton()***, and ***AsTransient()*** methods.

Each of these interfaces will be described in more detail in the topics that follow.

The actual concrete types of the ***BasicDI*** class library are defined as internal to the class library. The only public access to these
types is provided by the public interfaces listed above. There are three main concrete class types:

1. The ***Dependency\<T>*** class implements the ***IDependency\<T>***, ***ICanBindTo\<T>***, and ***ICanSpecifyLifetime*** interfaces.
1. The ***Scope*** class implements the ***IScope*** interface.
1. The ***Container*** class implements the ***IContainer*** interface.

## The *IDependency\<T>* Interface
The ***IDependency\<T>*** interface describes a single dependency type and its resolving type. The ***GetDependency\<T>()*** method of the
***IContainer*** interface provides the only means of obtaining an instance of this interface type.

### *IDependency\<T>* Properties
The ***IDependency\<T>*** interface defines the following properties.

#### *Factory* Property
The ***Factory*** property returns a reference to the factory that is used for creating instances of the resolving type, or *null* if no factory
method was registered when the dependency was added to the dependency injection container. The return type is ***Func\<T>*** where the type
parameter ***T*** specifies the dependency type.

```csharp
Func<T>? Factory { get; }
```

#### *Lifetime* Property
The ***Lifetime*** property returns the lifetime of the dependency as a ***DependencyLifetime*** enumeration value. The possible values are:
- ***DependencyLifetime.Undefined*** (0)
- ***DependencyLifetime.Singleton*** (1)
- ***DependencyLifetime.Scoped*** (2)
- ***DependencyLifetime.Transient*** (3)

```csharp
DependencyLifetime Lifetime { get; }
```

#### *ResolvingObject* Property
The ***ResolvingObject*** property returns *null* if the ***Lifetime*** property isn't ***DependencyLifetime.Singleton***. It will also return
*null* if ***IContainer.Resolve\<T>()*** method hasn't yet been called for this dependency type. Otherwise, a reference to the resolving object
will be returned. The return value will be cast to the dependency type.

```csharp
T? ResolvingObject { get; }
```

#### *ResolvingType* Property
The ***ResolvingType*** property returns a ***System.Type*** object describing the class type of the resolving object for this
***IDependency\<T>*** instance.

```csharp
Type ResolvingType { get; }
```

#### *Type* Property
The ***Type*** property returns a ***System.Type*** object describing the dependency type of this ***IDependency\<T>*** instance.

```csharp
Type Type { get; }
```

## The *IScope* Interface
The ***IScope*** interface represents a single dependency scope. Instances of this interface type must be used for resolving all scoped
dependencies. The ***CreateScope()*** method of the ***IContainer*** interface provides the only means of obtaining an instance of this
interface type.

> [!Note]
> *The **IScope** interface inherits from the **System.IDisposable** interface.*

### *IScope* Properties
The ***IScope*** interface defines a single property:

#### *Guid* Property
The ***Guid*** property returns a ***System.Guid*** value that uniquely identifies the dependency scope. The Guid value is generated automatically
every time the ***CreateScope()*** method is called to create a new ***IScope*** instance.

```csharp
Guid Guid { get; }
```

### *IScope* Methods
The ***IScope*** interface defines the following methods:

#### *Dispose()* Method
The ***Dispose()*** method is an implementation of the ***IDisposable*** interface that the ***IScope*** interface inherits from.

```csharp
void Dispose();
```

#### *Resolve\<T>()* Method
The ***Resolve\<T>()*** method is used to resolve scoped dependencies. The type parameter ***T*** is the dependency type being resolved. The
method returns an instance of the resolving type cast as the dependency type.

```csharp
T Resolve<T>() where T : class;
```

</br>

> [!Note]
> *The **Resolve\<T>()** method of the **IScope** interface can also be used to resolve singleton and transient dependencies the same as the
> **Resolve\<T>()** method of the **IContainer** interface.*

## The *IContainer* Interface
The ***IContainer*** interface provides access to the dependency injection container. This is the primary component of the ***BasicDI*** class
library. This component is responsible for:

- Creating new ***Dependency\<T>*** objects and adding them to the dependency injection container
- Creating new ***Scope*** objects and adding them to the dependency injection container
- Resolving dependencies

### *IContainer* Methods
The ***IContainer*** interface defines the following methods:

#### *Bind\<T>()* Method
The ***Bind\<T>()*** method is used to create a new ***Dependency\<T>*** object. The type parameter ***T*** specifies the dependency type.
This method is part of the fluent API. It is the first method that must be called when you are binding a given dependency type to a specific
resolving type. The method returns the ***Dependency\<T>*** object cast as an ***ICanBindTo\<T>*** interface object.

```csharp
ICanBindTo<T> Bind<T>() where T : class;
```

</br>

> [!Note]
> *The type parameter **T** specified on the **Bind\<T>()** method must be an interface type or a concrete class type. Abstract class types
> are not allowed.*

#### *CreateScope()* Method
The ***CreateScope()*** method creates a new ***Scope*** object representing a specific dependency scope. The ***Scope*** object is cast as
an ***IScope*** interface object and returned to the caller.

```csharp
IScope CreateScope();
```

#### *GetDependency\<T>()* Method
The ***GetDependency\<T>()*** method is used to retrieve an instance of the ***IDependency\<T>*** object from the dependency injection
container. The type parameter ***T*** is the type of dependency to look for. The ***IDependency\<T>*** object will be returned to the
caller if the dependency type has previously been added to the dependency injection container. Otherwise, *null* will be returned.

```csharp
IDependency<T>? GetDependency<T>() where T : class;
```

#### *Register\<T>()* Method
The ***Register\<T>()*** method is used to create a new ***Dependency\<T>*** object when we wish to register a concrete class type with
the dependency injection container. The type parameter ***T*** specifies the concrete class type to be registered. This method is part of
the fluent API. It must be the first method called when registering a concrete class type. The method returns the new ***Dependency\<T>***
object cast as an ***ICanSpecifyLifetime*** interface object.

The ***Register\<T>()*** method allows you to specify an optional parameter. If specified, this parameter must be a factory method that
returns the concrete class type ***T*** or a concrete class type that derives from that class type.

```csharp
ICanSpecifyLifetime Register<T>(Func<T>? factory = null) where T : class;
```

</br>

> [!Note]
> *The type parameter **T** specified on the **Bind\<T>()** method must be a concrete class type if the optional factory parameter isn't
> specified. It must not be an abstract class type. It may be an interface type if the optional factory parameter is supplied.*

#### *Resolve\<T>()* Method
The ***Resolve\<T>()*** method is used to retrieve the resolving instance for the given dependency type. The type parameter ***T***
specifies the dependency type to be resolved. The resolving instance is cast as the dependency type and returned to the caller.