# BasicDI
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

### Creating an Instance of the Dependency InjectionContainer
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

### Registering Dependencies with the Container
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

### Specifying a Factory Method
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

### Resolving Dependencies
After all dependencies have been registered with the dependency injection container, you can begin to ask for dependency objects from the container.
The ***Resolve\<T>()*** method is used to request the resolving object corresponding to a given dependency type. The type parameter ***T*** specifies
the dependency type being resolved.

*Example:*
```csharp
private IContainer container = Container.Current;
container.Bind<ILogger>().To<ConsoleLogger>().AsSingleton();
private ILogger logger = container.Resolve<ILogger>();
```