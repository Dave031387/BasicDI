namespace BasicDI;

using FluentAssertions;
using TestObjects;

public class BasicDIUnitTests
{
    [Fact]
    public void GetContainerInstance_ShouldInitializeContainer()
    {
        // Arrange/Act
        Container container = Container.TestInstance;

        // Assert
        container
            .Should()
            .NotBeNull();
        container._dependencies
            .Should()
            .NotBeNull();
    }

    [Fact]
    public void GetContainerMoreThanOnce_ShouldReturnSameInstanceEachTime()
    {
        // Arrange/Act
        Container container1 = Container.Current;
        Container container2 = Container.Current;
        Container container3 = Container.Current;

        // Assert
        container1
            .Should()
            .NotBeNull();
        container1
            .Should()
            .BeSameAs(container2);
        container2
            .Should()
            .BeSameAs(container3);
    }

    [Fact]
    public void BindDependencyType_ShouldCreateNewDependencyObject()
    {
        // Arrange
        Container container = Container.TestInstance;

        // Act
        Dependency<ISimpleObject> dependency = (Dependency<ISimpleObject>)container.Bind<ISimpleObject>();

        // Assert
        dependency
            .Should()
            .NotBeNull();
        dependency.Type
            .Should()
            .Be(typeof(ISimpleObject));
        dependency.Factory
            .Should()
            .BeNull();
        dependency.Lifetime
            .Should()
            .Be(DependencyLifetime.Undefined);
        dependency.ResolvingObject
            .Should()
            .BeNull();
        dependency.ResolvingType
            .Should()
            .Be(typeof(object));
        dependency._container
            .Should()
            .BeSameAs(container);
    }

    [Fact]
    public void BindDependencyTypeToResolvingTypeWithoutFactory_ShouldUpdateDependencyObject()
    {
        // Arrange
        Container container = Container.TestInstance;

        // Act
        Dependency<ISimpleObject> dependency = (Dependency<ISimpleObject>)container.Bind<ISimpleObject>().To<SimpleObject1>();

        // Assert
        dependency
            .Should()
            .NotBeNull();
        dependency.Type
            .Should()
            .Be(typeof(ISimpleObject));
        dependency.Factory
            .Should()
            .BeNull();
        dependency.Lifetime
            .Should()
            .Be(DependencyLifetime.Undefined);
        dependency.ResolvingObject
            .Should()
            .BeNull();
        dependency.ResolvingType
            .Should()
            .Be(typeof(SimpleObject1));
        dependency._container
            .Should()
            .BeSameAs(container);
    }

    [Fact]
    public void BindDependencyTypeToResolvingTypeWithFactory_ShouldUpdateDependencyObject()
    {
        // Arrange
        Container container = Container.TestInstance;
        static SimpleObject1 factory() => new();

        // Act
        Dependency<ISimpleObject> dependency = (Dependency<ISimpleObject>)container.Bind<ISimpleObject>().To<SimpleObject1>(factory);

        // Assert
        dependency
            .Should()
            .NotBeNull();
        dependency.Type
            .Should()
            .Be(typeof(ISimpleObject));
        dependency.Factory
            .Should()
            .BeSameAs(factory);
        dependency.Lifetime
            .Should()
            .Be(DependencyLifetime.Undefined);
        dependency.ResolvingObject
            .Should()
            .BeNull();
        dependency.ResolvingType
            .Should()
            .Be(typeof(SimpleObject1));
        dependency._container
            .Should()
            .BeSameAs(container);
    }

    [Fact]
    public void BindDependencyTypeToIncompatibleResolvingType_ShouldThrowException()
    {
        // Arrange
        Container container = Container.TestInstance;
        Func<ICanSpecifyLifetime> func = () => container.Bind<ISimpleObject>().To<OtherObject1>();
        string expectedMessage = string.Format(Messages.IncompatibleResolvingType, typeof(OtherObject1).FullName, typeof(ISimpleObject).FullName);

        // Act/Assert
        func
            .Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public void BindDependencyTypeToResolvingTypeThatIsNotAClass_ShouldThrowException()
    {
        // Arrange
        Container container = Container.TestInstance;
        Func<ICanSpecifyLifetime> func = () => container.Bind<ISimpleObject>().To<ISimpleObject2>();
        string expectedMessage = string.Format(Messages.ResolvingTypeNotConcreteClass, typeof(ISimpleObject2).FullName, typeof(ISimpleObject).FullName);

        // Act/Assert
        func
            .Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public void BindDependencyTypeToResolvingTypeThatIsAbstractClass_ShouldThrowException()
    {
        // Arrange
        Container container = Container.TestInstance;
        Func<ICanSpecifyLifetime> func = () => container.Bind<ISimpleObject>().To<AbstractClass>();
        string expectedMessage = string.Format(Messages.ResolvingTypeNotConcreteClass, typeof(AbstractClass).FullName, typeof(ISimpleObject).FullName);

        // Act/Assert
        func
            .Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public void RegisterDependencyTypeWithoutFactory_ShouldCreateNewDependencyObject()
    {
        // Arrange
        Container container = Container.TestInstance;

        // Act
        Dependency<SimpleObject1> dependency = (Dependency<SimpleObject1>)container.Register<SimpleObject1>();

        // Assert
        dependency
            .Should()
            .NotBeNull();
        dependency.Type
            .Should()
            .Be(typeof(SimpleObject1));
        dependency.Factory
            .Should()
            .BeNull();
        dependency.Lifetime
            .Should()
            .Be(DependencyLifetime.Undefined);
        dependency.ResolvingObject
            .Should()
            .BeNull();
        dependency.ResolvingType
            .Should()
            .Be(typeof(SimpleObject1));
        dependency._container
            .Should()
            .BeSameAs(container);
    }

    [Fact]
    public void RegisterDependencyTypeWithFactory_ShouldCreateNewDependencyObject()
    {
        // Arrange
        Container container = Container.TestInstance;
        static SimpleObject1 factory() => new();

        // Act
        Dependency<SimpleObject1> dependency = (Dependency<SimpleObject1>)container.Register(factory);

        // Assert
        dependency
            .Should()
            .NotBeNull();
        dependency.Type
            .Should()
            .Be(typeof(SimpleObject1));
        dependency.Factory
            .Should()
            .BeSameAs(factory);
        dependency.Lifetime
            .Should()
            .Be(DependencyLifetime.Undefined);
        dependency.ResolvingObject
            .Should()
            .BeNull();
        dependency.ResolvingType
            .Should()
            .Be(typeof(SimpleObject1));
        dependency._container
            .Should()
            .BeSameAs(container);
    }

    [Fact]
    public void RegisterDependencyThatIsNotAClassType_ShouldThrowException()
    {
        // Arrange
        Container container = Container.TestInstance;
        Func<ICanSpecifyLifetime> func = () => container.Register<ISimpleObject>();
        string expectedMessage = string.Format(Messages.RegisteredTypeNotConcreteClass, typeof(ISimpleObject));

        // Act/Assert
        func
            .Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public void RegisterDependencyThatIsAnAbstractClass_ShouldThrowException()
    {
        // Arrange
        Container container = Container.TestInstance;
        Func<ICanSpecifyLifetime> func = () => container.Register<AbstractClass>();
        string expectedMessage = string.Format(Messages.RegisteredTypeNotConcreteClass, typeof(AbstractClass));

        // Act/Assert
        func
            .Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public void BindGenericInterface_ShouldCreateNewDependencyObject()
    {
        // Arrange
        Container container = Container.TestInstance;

        // Act
        Dependency<IGenericObject<int>> dependency = (Dependency<IGenericObject<int>>)container.Bind<IGenericObject<int>>();

        // Assert
        dependency
            .Should()
            .NotBeNull();
        dependency.Type
            .Should()
            .Be(typeof(IGenericObject<int>));
        dependency.Factory
            .Should()
            .BeNull();
        dependency.Lifetime
            .Should()
            .Be(DependencyLifetime.Undefined);
        dependency.ResolvingObject
            .Should()
            .BeNull();
        dependency.ResolvingType
            .Should()
            .Be(typeof(object));
        dependency._container
            .Should()
            .BeSameAs(container);
    }

    [Fact]
    public void BindGenericTypeToGenericResolvingType_ShouldUpdateDependencyObject()
    {
        // Arrange
        Container container = Container.TestInstance;

        // Act
        Dependency<IGenericObject<string>> dependency = (Dependency<IGenericObject<string>>)container.Bind<IGenericObject<string>>().To<GenericObject<string>>();

        // Assert
        dependency
            .Should()
            .NotBeNull();
        dependency.Type
            .Should()
            .Be(typeof(IGenericObject<string>));
        dependency.Factory
            .Should()
            .BeNull();
        dependency.Lifetime
            .Should()
            .Be(DependencyLifetime.Undefined);
        dependency.ResolvingObject
            .Should()
            .BeNull();
        dependency.ResolvingType
            .Should()
            .Be(typeof(GenericObject<string>));
        dependency._container
            .Should()
            .BeSameAs(container);
    }

    [Fact]
    public void BindGenericTypeToIncompatibleResolvingType_ShouldThrowException()
    {
        // Arrange
        Container container = Container.TestInstance;
        Func<ICanSpecifyLifetime> func = () => container.Bind<IGenericObject<int>>().To<GenericObject<string>>();
        string expectedMessage = string.Format(Messages.IncompatibleResolvingType, typeof(GenericObject<string>).FullName, typeof(IGenericObject<int>).FullName);

        // Act/Assert
        func
            .Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public void RegisterGenericType_ShouldCreateNewDependencyObject()
    {
        // Arrange
        Container container = Container.TestInstance;

        // Act
        Dependency<GenericObject<bool>> dependency = (Dependency<GenericObject<bool>>)container.Register<GenericObject<bool>>();

        // Assert
        dependency
            .Should()
            .NotBeNull();
        dependency.Type
            .Should()
            .Be(typeof(GenericObject<bool>));
        dependency.Factory
            .Should()
            .BeNull();
        dependency.Lifetime
            .Should()
            .Be(DependencyLifetime.Undefined);
        dependency.ResolvingObject
            .Should()
            .BeNull();
        dependency.ResolvingType
            .Should()
            .Be(typeof(GenericObject<bool>));
        dependency._container
            .Should()
            .BeSameAs(container);
    }

    [Fact]
    public void BindToResolvingTypeAsScoped_ShouldUpdateDependencyAndAddToContainer()
    {
        // Arrange
        Container container = Container.TestInstance;

        // Act
        container.Bind<ISimpleObject>().To<SimpleObject1>().AsScoped();

        // Assert
        container._dependencies
            .Should()
            .ContainSingle();
        container._dependencies
            .Should()
            .ContainKey(typeof(ISimpleObject));
        Dependency<ISimpleObject> dependency = GetDependency<ISimpleObject>(container);
        dependency
            .Should()
            .NotBeNull();
        dependency.Type
            .Should()
            .Be(typeof(ISimpleObject));
        dependency.Factory
            .Should()
            .BeNull();
        dependency.Lifetime
            .Should()
            .Be(DependencyLifetime.Scoped);
        dependency.ResolvingObject
            .Should()
            .BeNull();
        dependency.ResolvingType
            .Should()
            .Be(typeof(SimpleObject1));
        dependency._container
            .Should()
            .BeSameAs(container);
    }

    [Fact]
    public void BindToResolvingTypeAsSingleton_ShouldUpdateDependencyAndAddToContainer()
    {
        // Arrange
        Container container = Container.TestInstance;

        // Act
        container.Bind<ISimpleObject>().To<SimpleObject1>().AsSingleton();

        // Assert
        container._dependencies
            .Should()
            .ContainSingle();
        container._dependencies
            .Should()
            .ContainKey(typeof(ISimpleObject));
        Dependency<ISimpleObject> dependency = GetDependency<ISimpleObject>(container);
        dependency
            .Should()
            .NotBeNull();
        dependency.Type
            .Should()
            .Be(typeof(ISimpleObject));
        dependency.Factory
            .Should()
            .BeNull();
        dependency.Lifetime
            .Should()
            .Be(DependencyLifetime.Singleton);
        dependency.ResolvingObject
            .Should()
            .BeNull();
        dependency.ResolvingType
            .Should()
            .Be(typeof(SimpleObject1));
        dependency._container
            .Should()
            .BeSameAs(container);
    }

    [Fact]
    public void BindToResolvingTypeAsTransient_ShouldUpdateDependencyAndAddToContainer()
    {
        // Arrange
        Container container = Container.TestInstance;

        // Act
        container.Bind<ISimpleObject>().To<SimpleObject1>().AsTransient();

        // Assert
        container._dependencies
            .Should()
            .ContainSingle();
        container._dependencies
            .Should()
            .ContainKey(typeof(ISimpleObject));
        Dependency<ISimpleObject> dependency = GetDependency<ISimpleObject>(container);
        dependency
            .Should()
            .NotBeNull();
        dependency.Type
            .Should()
            .Be(typeof(ISimpleObject));
        dependency.Factory
            .Should()
            .BeNull();
        dependency.Lifetime
            .Should()
            .Be(DependencyLifetime.Transient);
        dependency.ResolvingObject
            .Should()
            .BeNull();
        dependency.ResolvingType
            .Should()
            .Be(typeof(SimpleObject1));
        dependency._container
            .Should()
            .BeSameAs(container);
    }

    [Fact]
    public void GetDependencyForAnUnregisteredType_ShouldReturnNull()
    {
        // Arrange
        Container container = Container.TestInstance;

        // Act
        IDependency<ISimpleObject>? dependency = container.GetDependency<ISimpleObject>();

        // Assert
        dependency
            .Should()
            .BeNull();
    }

    [Fact]
    public void GetDependencyForRegisteredType_ShouldReturnDependencyObject()
    {
        // Arrange
        Container container = Container.TestInstance;
        container.Bind<ISimpleObject>().To<SimpleObject1>().AsSingleton();
        Dependency<ISimpleObject> expected = GetDependency<ISimpleObject>(container);

        // Act
        IDependency<ISimpleObject>? dependency = container.GetDependency<ISimpleObject>();

        // Assert
        dependency
            .Should()
            .BeSameAs(expected);
    }

    private static Dependency<T> GetDependency<T>(Container container) where T : class
        => (Dependency<T>)container._dependencies[typeof(T)];
}