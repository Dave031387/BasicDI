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
        container._scopes
            .Should()
            .NotBeNull();
        container._dependencies
            .Should()
            .BeEmpty();
        container._scopes
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void GetContainerMoreThanOnce_ShouldReturnSameInstanceEachTime()
    {
        // Arrange/Act
        IContainer container1 = Container.Current;
        IContainer container2 = Container.Current;
        IContainer container3 = Container.Current;

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
    public void BindDependencyTypeThatIsAnInterface_ShouldCreateNewDependencyObject()
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
    public void BindDependencyTypeThatIsConcreteClass_ShouldCreateNewDependencyObject()
    {
        // Arrange
        Container container = Container.TestInstance;

        // Act
        Dependency<SimpleObject> dependency = (Dependency<SimpleObject>)container.Bind<SimpleObject>();

        // Assert
        dependency
            .Should()
            .NotBeNull();
        dependency.Type
            .Should()
            .Be(typeof(SimpleObject));
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
    public void BindAbstractType_ShouldThrowException()
    {
        // Arrange
        Container container = Container.TestInstance;
        Func<ICanBindTo<AbstractClass>> func = container.Bind<AbstractClass>;
        string expectedMessage = string.Format(Messages.DependencyTypeNotValid, typeof(AbstractClass).FullName);

        // Act/Assert
        func
            .Should()
            .ThrowExactly<DependencyInjectionException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public void BindDependencyTypeToItself_ShouldUpdateDependencyObject()
    {
        // Arrange
        Container container = Container.TestInstance;

        // Act
        Dependency<SimpleObject> dependency = (Dependency<SimpleObject>)container.Bind<SimpleObject>().To<SimpleObject>();

        // Assert
        dependency
            .Should()
            .NotBeNull();
        dependency.Type
            .Should()
            .Be(typeof(SimpleObject));
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
            .Be(typeof(SimpleObject));
        dependency._container
            .Should()
            .BeSameAs(container);
    }

    [Fact]
    public void BindDependencyTypeToDerivedClass_ShouldUpdateDependencyObject()
    {
        // Arrange
        Container container = Container.TestInstance;

        // Act
        Dependency<SimpleObject> dependency = (Dependency<SimpleObject>)container.Bind<SimpleObject>().To<DerivedObject>();

        // Assert
        dependency
            .Should()
            .NotBeNull();
        dependency.Type
            .Should()
            .Be(typeof(SimpleObject));
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
            .Be(typeof(DerivedObject));
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
        Dependency<ISimpleObject> dependency = (Dependency<ISimpleObject>)container.Bind<ISimpleObject>().To<SimpleObject>();

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
            .Be(typeof(SimpleObject));
        dependency._container
            .Should()
            .BeSameAs(container);
    }

    [Fact]
    public void BindDependencyTypeToResolvingTypeWithFactory_ShouldUpdateDependencyObject()
    {
        // Arrange
        Container container = Container.TestInstance;
        static SimpleObject factory() => new();

        // Act
        Dependency<ISimpleObject> dependency = (Dependency<ISimpleObject>)container.Bind<ISimpleObject>().To<SimpleObject>(factory);

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
            .Be(typeof(SimpleObject));
        dependency._container
            .Should()
            .BeSameAs(container);
    }

    [Fact]
    public void BindDependencyTypeToIncompatibleResolvingType_ShouldThrowException()
    {
        // Arrange
        Container container = Container.TestInstance;
        Func<ICanSpecifyLifetime> func = () => container.Bind<ISimpleObject>().To<OtherObject>();
        string expectedMessage = string.Format(Messages.IncompatibleResolvingType, typeof(OtherObject).FullName, typeof(ISimpleObject).FullName);

        // Act/Assert
        func
            .Should()
            .ThrowExactly<DependencyInjectionException>()
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
            .ThrowExactly<DependencyInjectionException>()
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
            .ThrowExactly<DependencyInjectionException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public void RegisterConcreteDependencyTypeWithoutFactory_ShouldCreateNewDependencyObject()
    {
        // Arrange
        Container container = Container.TestInstance;

        // Act
        Dependency<SimpleObject> dependency = (Dependency<SimpleObject>)container.Register<SimpleObject>();

        // Assert
        dependency
            .Should()
            .NotBeNull();
        dependency.Type
            .Should()
            .Be(typeof(SimpleObject));
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
            .Be(typeof(SimpleObject));
        dependency._container
            .Should()
            .BeSameAs(container);
    }

    [Fact]
    public void RegisterConcreteDependencyTypeWithFactory_ShouldCreateNewDependencyObject()
    {
        // Arrange
        Container container = Container.TestInstance;
        static SimpleObject factory() => new();

        // Act
        Dependency<SimpleObject> dependency = (Dependency<SimpleObject>)container.Register(factory);

        // Assert
        dependency
            .Should()
            .NotBeNull();
        dependency.Type
            .Should()
            .Be(typeof(SimpleObject));
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
            .Be(typeof(SimpleObject));
        dependency._container
            .Should()
            .BeSameAs(container);
    }

    [Fact]
    public void RegisterDependencyThatIsAnInterfaceWithoutFactory_ShouldThrowException()
    {
        // Arrange
        Container container = Container.TestInstance;
        Func<ICanSpecifyLifetime> func = () => container.Register<ISimpleObject>();
        string expectedMessage = string.Format(Messages.RegisteredTypeNotConcreteClass, typeof(ISimpleObject));

        // Act/Assert
        func
            .Should()
            .ThrowExactly<DependencyInjectionException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public void RegisterDependencyThatIsAnInterfaceWithFactory_ShouldCreateNewDependencyObject()
    {
        // Arrange
        Container container = Container.TestInstance;
        static ISimpleObject factory() => new SimpleObject();

        // Act
        Dependency<ISimpleObject> dependency = (Dependency<ISimpleObject>)container.Register(factory);

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
            .Be(typeof(ISimpleObject));
        dependency._container
            .Should()
            .BeSameAs(container);
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
            .ThrowExactly<DependencyInjectionException>()
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
            .ThrowExactly<DependencyInjectionException>()
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
        container.Bind<ISimpleObject>().To<SimpleObject>().AsScoped();

        // Assert
        container._scopes
            .Should()
            .BeEmpty();
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
            .Be(typeof(SimpleObject));
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
        container.Bind<ISimpleObject>().To<SimpleObject>().AsSingleton();

        // Assert
        container._scopes
            .Should()
            .BeEmpty();
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
            .Be(typeof(SimpleObject));
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
        container.Bind<ISimpleObject>().To<SimpleObject>().AsTransient();

        // Assert
        container._scopes
            .Should()
            .BeEmpty();
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
            .Be(typeof(SimpleObject));
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
        container.Bind<ISimpleObject>().To<SimpleObject>().AsSingleton();
        Dependency<ISimpleObject> expected = GetDependency<ISimpleObject>(container);

        // Act
        IDependency<ISimpleObject>? dependency = container.GetDependency<ISimpleObject>();

        // Assert
        dependency
            .Should()
            .BeSameAs(expected);
    }

    [Fact]
    public void CreateScope_ShouldCreateNewScopeAndAddToScopeList()
    {
        // Arrange
        Container container = Container.TestInstance;

        // Act
        IScope scope = container.CreateScope();

        // Assert
        scope
            .Should()
            .NotBeNull();
        container._scopes
            .Should()
            .ContainSingle();
        container._scopes
            .Should()
            .ContainKey(scope.Guid);
        container._scopes[scope.Guid]
            .Should()
            .BeSameAs(scope);
    }

    [Fact]
    public void CreateMultipleScopes_EachScopeShouldHaveUniqueGuidAndShouldBeAddedToScopeList()
    {
        // Arrange
        Container container = Container.TestInstance;

        // Act
        IScope scope1 = container.CreateScope();
        IScope scope2 = container.CreateScope();
        IScope scope3 = container.CreateScope();

        // Assert
        scope1.Guid
            .Should()
            .NotBe(scope2.Guid);
        scope2.Guid
            .Should()
            .NotBe(scope3.Guid);
        scope3.Guid
            .Should()
            .NotBe(scope1.Guid);
        container._scopes
            .Should()
            .HaveCount(3);
        container._scopes
            .Should()
            .ContainKeys(scope1.Guid, scope2.Guid, scope3.Guid);
        container._scopes[scope1.Guid]
            .Should()
            .BeSameAs(scope1);
        container._scopes[scope2.Guid]
            .Should()
            .BeSameAs(scope2);
        container._scopes[scope3.Guid]
            .Should()
            .BeSameAs(scope3);
    }

    [Fact]
    public void ResolveSimpleDependency_ShouldReturnInstanceOfResolvingType()
    {
        // Arrange
        Container container = Container.TestInstance;
        container.Bind<ISimpleObject>().To<SimpleObject>().AsTransient();

        // Act
        ISimpleObject simpleObject = container.Resolve<ISimpleObject>();

        // Assert
        simpleObject
            .Should()
            .NotBeNull();
        simpleObject
            .Should()
            .BeOfType<SimpleObject>();
    }

    [Fact]
    public void ResolveDependencyWithoutDefinedLifetime_ShouldThrowException()
    {
        // Arrange
        Container container = Container.TestInstance;
        container.Bind<ISimpleObject>().To<SimpleObject>().AsTransient();
        ((Dependency<ISimpleObject>)container._dependencies[typeof(ISimpleObject)]).Lifetime = DependencyLifetime.Undefined;
        Func<ISimpleObject> func = container.Resolve<ISimpleObject>;
        string expectedMessage = Messages.InvalidLifetime;

        // Act/Assert
        func
            .Should()
            .ThrowExactly<DependencyInjectionException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public void ResolveUnknownDependency_ShouldThrowException()
    {
        // Arrange
        Container container = Container.TestInstance;
        Func<ISimpleObject> func = container.Resolve<ISimpleObject>;
        string expectedMessage = string.Format(Messages.UnableToResolveUnknownDependency, typeof(ISimpleObject).FullName);

        // Act/Assert
        func
            .Should()
            .ThrowExactly<DependencyInjectionException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public void NoConstructorsFoundWhenResolvingDependency_ShouldThrowException()
    {
        // Arrange
        Container container = Container.TestInstance;
        container.Register<SimpleObject>().AsSingleton();
        ((Dependency<SimpleObject>)container._dependencies[typeof(SimpleObject)]).ResolvingType = typeof(StaticClass);
        Func<SimpleObject> func = container.Resolve<SimpleObject>;
        string expectedMessage = string.Format(Messages.NoConstructorsFound, typeof(StaticClass));

        // Act/Assert
        func
            .Should()
            .ThrowExactly<DependencyInjectionException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public void ExceptionThrownDuringDependencyConstruction_ShouldReThrowException()
    {
        // Arrange
        Container container = Container.TestInstance;
        container.Bind<ISimpleObject>().To<ConstructorException>().AsTransient();
        Func<ISimpleObject> func = container.Resolve<ISimpleObject>;
        string innerMessage = "Exception has been thrown by the target of an invocation.";
        string expectedMessage = string.Format(Messages.FailedToConstructResolvingObject, typeof(ISimpleObject).FullName, innerMessage);

        // Act/Assert
        func
            .Should()
            .ThrowExactly<DependencyInjectionException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public void ResolveTransientDependencyMoreThanOnce_ShouldReturnNewResolvingInstanceEachTime()
    {
        // Arrange
        Container container = Container.TestInstance;
        container.Bind<ISimpleObject>().To<SimpleObject>().AsTransient();

        // Act
        ISimpleObject simpleObject1 = container.Resolve<ISimpleObject>();
        ISimpleObject simpleObject2 = container.Resolve<ISimpleObject>();
        ISimpleObject simpleObject3 = container.Resolve<ISimpleObject>();

        // Assert
        simpleObject1
            .Should()
            .NotBeNull();
        simpleObject2
            .Should()
            .NotBeNull();
        simpleObject3
            .Should()
            .NotBeNull();
        simpleObject1
            .Should()
            .NotBeSameAs(simpleObject2);
        simpleObject2
            .Should()
            .NotBeSameAs(simpleObject3);
        simpleObject3
            .Should()
            .NotBeSameAs(simpleObject1);
    }

    [Fact]
    public void ResolveSingletonDependencyMoreThanOnce_ShouldReturnSameInstanceEachTime()
    {
        // Arrange
        Container container = Container.TestInstance;
        container.Bind<ISimpleObject>().To<SimpleObject>().AsSingleton();

        // Act
        ISimpleObject simpleObject1 = container.Resolve<ISimpleObject>();
        ISimpleObject simpleObject2 = container.Resolve<ISimpleObject>();
        ISimpleObject simpleObject3 = container.Resolve<ISimpleObject>();

        // Assert
        simpleObject1
            .Should()
            .NotBeNull();
        simpleObject2
            .Should()
            .NotBeNull();
        simpleObject3
            .Should()
            .NotBeNull();
        simpleObject1
            .Should()
            .BeSameAs(simpleObject2);
        simpleObject1
            .Should()
            .BeSameAs(simpleObject3);
    }

    [Fact]
    public void ResolvingScopedDependencyOutsideOfScope_ShouldThrowException()
    {
        // Arrange
        Container container = Container.TestInstance;
        container.Bind<ISimpleObject>().To<SimpleObject>().AsScoped();
        Func<ISimpleObject> func = container.Resolve<ISimpleObject>;
        string expectedMessage = string.Format(Messages.ResolvingScopedDependencyOutsideOfScope, typeof(ISimpleObject).FullName);

        // Act/Assert
        func
            .Should()
            .ThrowExactly<DependencyInjectionException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public void ResolveDependencyHavingConstructorDependencies_ShouldResolveAllDependencies()
    {
        // Arrange
        Container container = Container.TestInstance;
        container.Bind<ISimpleObject>().To<SimpleObject>().AsSingleton();
        container.Bind<IOtherObject>().To<OtherObject>().AsTransient();

        // Act
        IOtherObject otherObject = container.Resolve<IOtherObject>();

        // Assert
        otherObject
            .Should()
            .NotBeNull();
        otherObject
            .Should()
            .BeOfType<OtherObject>();
        otherObject.SimpleObject
            .Should()
            .NotBeNull();
        otherObject.SimpleObject
            .Should()
            .BeOfType<SimpleObject>();
    }

    [Fact]
    public void ResolveComplexDependencyContainingGenericDependency_ShouldResolveAllDependencies()
    {
        // Arrange
        Container container = Container.TestInstance;
        container.Bind<ISimpleObject>().To<SimpleObject>().AsSingleton();
        container.Bind<IOtherObject>().To<OtherObject>().AsTransient();
        container.Bind<IGenericObject<ISimpleObject>>().To<GenericObject<ISimpleObject>>().AsSingleton();
        container.Bind<IComplexObject>().To<ComplexObject>().AsSingleton();

        // Act
        IComplexObject complexObject = container.Resolve<IComplexObject>();

        // Assert
        complexObject
            .Should()
            .NotBeNull();
        complexObject
            .Should()
            .BeOfType<ComplexObject>();
        complexObject.SimpleObject
            .Should()
            .NotBeNull();
        complexObject.SimpleObject
            .Should()
            .BeOfType<SimpleObject>();
        complexObject.OtherObject
            .Should()
            .NotBeNull();
        complexObject.OtherObject
            .Should()
            .BeOfType<OtherObject>();
        complexObject.OtherObject.SimpleObject
            .Should()
            .NotBeNull();
        complexObject.OtherObject.SimpleObject
            .Should()
            .BeSameAs(complexObject.SimpleObject);
        complexObject.GenericObject
            .Should()
            .NotBeNull();
        complexObject.GenericObject
            .Should()
            .BeOfType<GenericObject<ISimpleObject>>();
    }

    [Fact]
    public void ResolveDependencyThatHasFactoryDefined_ShouldUseFactoryToCreateResolvingInstance()
    {
        // Arrange
        Container container = Container.TestInstance;
        string expected = "Built by factory";
        IGenericObject<string> func() => new GenericObject<string>() { Value = expected };
        container.Bind<IGenericObject<string>>().To<GenericObject<string>>(func).AsTransient();

        // Act
        IGenericObject<string> genericObject = container.Resolve<IGenericObject<string>>();

        // Assert
        genericObject
            .Should()
            .NotBeNull();
        genericObject
            .Should()
            .BeOfType<GenericObject<string>>();
        genericObject.Value
            .Should()
            .NotBeNull();
        genericObject.Value
            .Should()
            .Be(expected);
    }

    [Fact]
    public void CreateNewDependencyScope_ShouldCreateScopeAndAddToScopeList()
    {
        // Arrange
        Container container = Container.TestInstance;

        // Act
        using (IScope scope = container.CreateScope())
        {
            // Assert
            scope
                .Should()
                .NotBeNull();
            scope.Guid
                .Should()
                .NotBe(Guid.Empty);
            ((Scope)scope)._container
                .Should()
                .BeSameAs(container);
            container._scopes
                .Should()
                .ContainSingle();
            container._scopes
                .Should()
                .ContainKey(scope.Guid);
            container._scopes[scope.Guid]
                .Should()
                .BeSameAs(scope);
        }
    }

    [Fact]
    public void AtEndOfDependencyScope_ShouldDiscardScope()
    {
        // Arrange
        Container container = Container.TestInstance;

        // Act
        using (IScope scope = container.CreateScope())
        {
            container._scopes
                .Should()
                .NotBeEmpty();
        }

        // Assert
        container._scopes
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void ManuallyDisposeScope_ShouldDiscardScope()
    {
        // Arrange
        Container container = Container.TestInstance;

        using (IScope scope = container.CreateScope())
        {
            container._scopes
                .Should()
                .NotBeEmpty();

            // Act
            scope.Dispose();

            // Assert
            container._scopes
                .Should()
                .BeEmpty();
        }
    }

    [Fact]
    public void ResolveScopedDependency_ShouldReturnTheResolvingObject()
    {
        // Arrange
        Container container = Container.TestInstance;
        container.Bind<ISimpleObject>().To<SimpleObject>().AsScoped();

        // Act
        using (IScope scope = container.CreateScope())
        {
            ISimpleObject simpleObject = scope.Resolve<ISimpleObject>();

            // Assert
            simpleObject
                .Should()
                .NotBeNull();
            simpleObject
                .Should()
                .BeOfType<SimpleObject>();
            ((Scope)scope)._resolvingObjects
                .Should()
                .NotBeEmpty();
            ((Scope)scope)._resolvingObjects
                .Should()
                .ContainKey(typeof(ISimpleObject));
            ((Scope)scope)._resolvingObjects[typeof(ISimpleObject)]
                .Should()
                .BeSameAs(simpleObject);
        }

        container._scopes
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void ResolveSameScopedDependencyMoreThanOnceInSameScope_ShouldReturnSameResolvingObjectInstance()
    {
        // Arrange
        Container container = Container.TestInstance;
        container.Bind<ISimpleObject>().To<SimpleObject>().AsScoped();

        // Act
        using (IScope scope = container.CreateScope())
        {
            ISimpleObject simpleObject1 = scope.Resolve<ISimpleObject>();
            ISimpleObject simpleObject2 = scope.Resolve<ISimpleObject>();

            // Assert
            simpleObject1
                .Should()
                .BeSameAs(simpleObject2);
        }
    }

    [Fact]
    public void ResolveSameScopedDependencyInDifferentScopes_ShouldReturnDifferentInstancesOfResolvingObject()
    {
        // Arrange
        Container container = Container.TestInstance;
        container.Bind<ISimpleObject>().To<SimpleObject>().AsScoped();

        // Act
        using (IScope scope1 = container.CreateScope())
        {
            ISimpleObject simpleObject1 = scope1.Resolve<ISimpleObject>();

            using (IScope scope2 = container.CreateScope())
            {
                ISimpleObject simpleObject2 = scope2.Resolve<ISimpleObject>();

                // Assert
                container._scopes
                    .Should()
                    .HaveCount(2);
                container._scopes
                    .Should()
                    .ContainKeys(scope1.Guid, scope2.Guid);
                simpleObject1
                    .Should()
                    .NotBeNull();
                simpleObject2
                    .Should()
                    .NotBeNull();
                simpleObject1
                    .Should()
                    .NotBeSameAs(simpleObject2);
            }

            container._scopes
                .Should()
                .ContainSingle();
            container._scopes
                .Should()
                .ContainKey(scope1.Guid);
            simpleObject1
                .Should()
                .NotBeNull();
        }

        container._scopes
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void ResolveSingletonInDifferentScopes_ShouldReturnSameInstanceEachTime()
    {
        // Arrange
        Container container = Container.TestInstance;
        container.Bind<ISimpleObject>().To<SimpleObject>().AsSingleton();

        // Act
        using (IScope scop1 = container.CreateScope())
        {
            ISimpleObject simpleObject1 = scop1.Resolve<ISimpleObject>();

            using (IScope scope2 = container.CreateScope())
            {
                ISimpleObject simpleObject2 = scope2.Resolve<ISimpleObject>();

                // Assert
                simpleObject1
                    .Should()
                    .BeSameAs(simpleObject2);
            }
        }
    }

    [Fact]
    public void ResolveScopedDependencyHavingNestedDependencies_ShouldResolveAllDependencies()
    {
        // Arrange
        Container container = Container.TestInstance;
        container.Bind<ISimpleObject>().To<SimpleObject>().AsTransient();
        container.Bind<IGenericObject<ISimpleObject>>().To<GenericObject<ISimpleObject>>().AsScoped();
        container.Bind<IOtherObject>().To<OtherObject>().AsScoped();
        container.Bind<IComplexObject>().To<ComplexObject>().AsScoped();

        // Act
        using (IScope scope = container.CreateScope())
        {
            IComplexObject complexObject = scope.Resolve<IComplexObject>();

            // Assert
            Scope thisScope = (Scope)scope;
            Type genericType = typeof(IGenericObject<ISimpleObject>);
            Type otherType = typeof(IOtherObject);
            Type complexType = typeof(IComplexObject);
            container._scopes
                .Should()
                .ContainSingle();
            thisScope._resolvingObjects
                .Should()
                .HaveCount(3);
            thisScope._resolvingObjects
                .Should()
                .ContainKeys(genericType, otherType, complexType);
            IGenericObject<ISimpleObject> genericObject = GetScopedResolvingObject<IGenericObject<ISimpleObject>>(thisScope);
            IOtherObject otherObject = GetScopedResolvingObject<IOtherObject>(thisScope);
            IComplexObject complexObject1 = GetScopedResolvingObject<IComplexObject>(thisScope);
            complexObject
                .Should()
                .NotBeNull();
            complexObject
                .Should()
                .BeSameAs(complexObject1);
            otherObject
                .Should()
                .NotBeNull();
            genericObject
                .Should()
                .NotBeNull();
            otherObject.SimpleObject
                .Should()
                .NotBeSameAs(complexObject.SimpleObject);
        }
    }

    [Fact]
    public void GetScopedResolvingObject_ShouldReturnNullIfDependencyNotResolved()
    {
        // Arrange
        Container container = Container.TestInstance;
        Scope scope = new(container);

        // Act
        ISimpleObject? simpleObject = scope.GetResolvingObject<ISimpleObject>();

        // Assert
        simpleObject
            .Should()
            .BeNull();
    }

    [Fact]
    public void ResolvingObjectHasMultipleConstructors_ShouldUseConstructorWithMostParameters()
    {
        // Arrange
        Container container = Container.TestInstance;
        container.Bind<ISimpleObject>().To<SimpleObject>().AsSingleton();
        container.Bind<IOtherObject>().To<OtherObject>().AsSingleton();
        container.Bind<IMultipleConstructors>().To<MultipleConstructors>().AsSingleton();

        // Act
        IMultipleConstructors multipleConstructors = container.Resolve<IMultipleConstructors>();

        // Assert
        multipleConstructors
            .Should()
            .NotBeNull();
        multipleConstructors.OtherObject
            .Should()
            .NotBeNull();
        multipleConstructors.SimpleObject
            .Should()
            .NotBeNull();
    }

    private static Dependency<T> GetDependency<T>(Container container) where T : class
        => (Dependency<T>)container._dependencies[typeof(T)];

    private static T GetScopedResolvingObject<T>(Scope scope) where T : class
        => (T)scope._resolvingObjects[typeof(T)];
}