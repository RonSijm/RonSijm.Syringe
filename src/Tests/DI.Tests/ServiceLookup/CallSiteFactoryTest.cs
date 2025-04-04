// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.DotNet.RemoteExecutor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using MicrosoftCopy.DependencyInjection.Tests.ServiceLookup.Types;
using RonSijm.Syringe.ServiceLookup;
using Xunit;

namespace MicrosoftCopy.DependencyInjection.Tests.ServiceLookup;

public class CallSiteFactoryTest
{
    [Fact]
    public void GetService_FactoryCallSite_Transient_DoesNotFail()
    {
        var collection = new ServiceCollection();
        collection.Add(ServiceDescriptor.Describe(typeof(FakeService), _ => new FakeService(), ServiceLifetime.Transient));
        collection.Add(ServiceDescriptor.Describe(typeof(IFakeService), _ => new FakeService(), ServiceLifetime.Transient));

        using var serviceProvider = collection.BuildServiceProvider(ServiceProviderMode.Dynamic);
        var expectedType = typeof(FakeService);

        Assert.Equal(expectedType, serviceProvider.GetService(typeof(IFakeService)).GetType());
        Assert.Equal(expectedType, serviceProvider.GetService(typeof(FakeService)).GetType());

        for (var i = 0; i < 50; i++)
        {
            Assert.Equal(expectedType, serviceProvider.GetService(typeof(IFakeService)).GetType());
            Assert.Equal(expectedType, serviceProvider.GetService(typeof(FakeService)).GetType());
            Thread.Sleep(10); // Give the background thread time to compile
        }
    }

    [Fact]
    public void CreateCallSite_Throws_IfTypeHasNoPublicConstructors()
    {
        // Arrange
        var type = typeof(TypeWithNoPublicConstructors);
        var expectedMessage = $"A suitable constructor for type '{type}' could not be located. " +
                              "Ensure the type is concrete and services are registered for all parameters of a public constructor.";
        var descriptor = new ServiceDescriptor(type, type, ServiceLifetime.Transient);
        var callSiteFactory = GetCallSiteFactory(descriptor);

        // Act and Assert
        var ex = Assert.Throws<InvalidOperationException>(() => callSiteFactory(type));
        Assert.Equal(expectedMessage, ex.Message);
    }

    [Theory]
    [InlineData(typeof(TypeWithNoConstructors))]
    [InlineData(typeof(TypeWithParameterlessConstructor))]
    [InlineData(typeof(TypeWithParameterlessPublicConstructor))]
    public void CreateCallSite_CreatesInstanceCallSite_IfTypeHasDefaultOrPublicParameterlessConstructor(Type type)
    {
        // Arrange
        var descriptor = new ServiceDescriptor(type, type, ServiceLifetime.Transient);
        var callSiteFactory = GetCallSiteFactory(descriptor);

        // Act
        var callSite = callSiteFactory(type);

        // Assert
        Assert.Equal(CallSiteResultCacheLocation.Dispose, callSite.Cache.Location);
        var ctroCallSite = Assert.IsType<ConstructorCallSite>(callSite);
        Assert.Empty(ctroCallSite.ParameterCallSites);
    }

    [Theory]
    [InlineData(typeof(TypeWithParameterizedConstructor))]
    [InlineData(typeof(TypeWithParameterizedAndNullaryConstructor))]
    [InlineData(typeof(TypeWithMultipleParameterizedConstructors))]
    [InlineData(typeof(TypeWithSupersetConstructors))]
    public void CreateCallSite_CreatesConstructorCallSite_IfTypeHasConstructorWithInjectableParameters(Type type)
    {
        // Arrange
        var descriptor = new ServiceDescriptor(type, type, ServiceLifetime.Transient);

        var callSiteFactory = GetCallSiteFactory(
            descriptor,
            new ServiceDescriptor(typeof(IFakeService), new FakeService())
        );

        // Act
        var callSite = callSiteFactory(type);

        // Assert
        Assert.Equal(CallSiteResultCacheLocation.Dispose, callSite.Cache.Location);
        var constructorCallSite = Assert.IsType<ConstructorCallSite>(callSite);
        Assert.Equal([typeof(IFakeService)], GetParameters(constructorCallSite));
    }

    [Fact]
    public void CreateCallSite_CreatesConstructorWithEnumerableParameters()
    {
        // Arrange
        var type = typeof(TypeWithEnumerableConstructors);
        var descriptor = new ServiceDescriptor(type, type, ServiceLifetime.Transient);

        var callSiteFactory = GetCallSiteFactory(
            descriptor,
            new ServiceDescriptor(typeof(IFakeService), new FakeService())
        );

        // Act
        var callSite = callSiteFactory(type);

        // Assert
        Assert.Equal(CallSiteResultCacheLocation.Dispose, callSite.Cache.Location);
        var constructorCallSite = Assert.IsType<ConstructorCallSite>(callSite);
        Assert.Equal(
            [typeof(IEnumerable<IFakeService>), typeof(IEnumerable<IFactoryService>)],
            GetParameters(constructorCallSite));
    }

    [Fact]
    public void CreateCallSite_UsesNullaryConstructorIfServicesCannotBeInjectedIntoOtherConstructors()
    {
        // Arrange
        var type = typeof(TypeWithParameterizedAndNullaryConstructor);
        var descriptor = new ServiceDescriptor(type, type, ServiceLifetime.Transient);
        var callSiteFactory = GetCallSiteFactory(descriptor);

        // Act
        var callSite = callSiteFactory(type);

        // Assert
        Assert.Equal(CallSiteResultCacheLocation.Dispose, callSite.Cache.Location);
        var ctorCallSite = Assert.IsType<ConstructorCallSite>(callSite);
        Assert.Empty(ctorCallSite.ParameterCallSites);
    }

    [Fact]
    public void CreateCallSite_Throws_IfClosedTypeDoesNotSatisfyStructGenericConstraint()
    {
        // Arrange
        var serviceType = typeof(IFakeOpenGenericService<>);
        var implementationType = typeof(ClassWithStructConstraint<>);
        var descriptor = new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient);
        var callSiteFactory = GetCallSiteFactory(descriptor);
        // Act
        var nonMatchingType = typeof(IFakeOpenGenericService<object>);
        // Assert
        Assert.Throws<ArgumentException>(() => callSiteFactory(nonMatchingType));
    }

    [Fact]
    public void CreateCallSite_ReturnsService_IfClosedTypeSatisfiesStructGenericConstraint()
    {
        // Arrange
        var serviceType = typeof(IFakeOpenGenericService<>);
        var implementationType = typeof(ClassWithStructConstraint<>);
        var descriptor = new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient);
        var callSiteFactory = GetCallSiteFactory(descriptor);
        // Act
        var matchingType = typeof(IFakeOpenGenericService<int>);
        var matchingCallSite = callSiteFactory(matchingType);
        // Assert
        Assert.NotNull(matchingCallSite);
    }

    [Fact]
    public void CreateCallSite_Throws_IfClosedTypeDoesNotSatisfyClassGenericConstraint()
    {
        // Arrange
        var serviceType = typeof(IFakeOpenGenericService<>);
        var implementationType = typeof(ClassWithClassConstraint<>);
        var descriptor = new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient);
        var callSiteFactory = GetCallSiteFactory(descriptor);
        // Act
        var nonMatchingType = typeof(IFakeOpenGenericService<int>);
        // Assert
        Assert.Throws<ArgumentException>(() => callSiteFactory(nonMatchingType));
    }

    [Fact]
    public void CreateCallSite_ReturnsService_IfClosedTypeSatisfiesClassGenericConstraint()
    {
        // Arrange
        var serviceType = typeof(IFakeOpenGenericService<>);
        var implementationType = typeof(ClassWithClassConstraint<>);
        var descriptor = new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient);
        var callSiteFactory = GetCallSiteFactory(descriptor);
        // Act
        var matchingType = typeof(IFakeOpenGenericService<object>);
        var matchingCallSite = callSiteFactory(matchingType);
        // Assert
        Assert.NotNull(matchingCallSite);
    }

    [Fact]
    public void CreateCallSite_Throws_IfClosedTypeDoesNotSatisfyNewGenericConstraint()
    {
        // Arrange
        var serviceType = typeof(IFakeOpenGenericService<>);
        var implementationType = typeof(ClassWithNewConstraint<>);
        var descriptor = new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient);
        var callSiteFactory = GetCallSiteFactory(descriptor);
        // Act
        var nonMatchingType = typeof(IFakeOpenGenericService<TypeWithNoPublicConstructors>);
        // Assert
        Assert.Throws<ArgumentException>(() => callSiteFactory(nonMatchingType));
    }

    [Fact]
    public void CreateCallSite_ReturnsService_IfClosedTypeSatisfiesNewGenericConstraint()
    {
        // Arrange
        var serviceType = typeof(IFakeOpenGenericService<>);
        var implementationType = typeof(ClassWithNewConstraint<>);
        var descriptor = new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient);
        var callSiteFactory = GetCallSiteFactory(descriptor, new ServiceDescriptor(typeof(TypeWithParameterlessPublicConstructor), new TypeWithParameterlessPublicConstructor()));
        // Act
        var matchingType = typeof(IFakeOpenGenericService<TypeWithParameterlessPublicConstructor>);
        var matchingCallSite = callSiteFactory(matchingType);
        // Assert
        Assert.NotNull(matchingCallSite);
    }

    [Fact]
    public void CreateCallSite_Throws_IfClosedTypeDoesNotSatisfyInterfaceGenericConstraint()
    {
        // Arrange
        var serviceType = typeof(IFakeOpenGenericService<>);
        var implementationType = typeof(ClassWithInterfaceConstraint<>);
        var descriptor = new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient);
        var callSiteFactory = GetCallSiteFactory(descriptor);
        // Act
        var nonMatchingType = typeof(IFakeOpenGenericService<int>);
        // Assert
        Assert.Throws<ArgumentException>(() => callSiteFactory(nonMatchingType));
    }

    [Fact]
    public void CreateCallSite_ReturnsService_IfClosedTypeSatisfiesInterfaceGenericConstraint()
    {
        // Arrange
        var serviceType = typeof(IFakeOpenGenericService<>);
        var implementationType = typeof(ClassWithInterfaceConstraint<>);
        var descriptor = new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient);
        var callSiteFactory = GetCallSiteFactory(descriptor, new ServiceDescriptor(typeof(string), ""));
        // Act
        var matchingType = typeof(IFakeOpenGenericService<string>);
        var matchingCallSite = callSiteFactory(matchingType);
        // Assert
        Assert.NotNull(matchingCallSite);
    }

    [Fact]
    public void CreateCallSite_Throws_IfClosedTypeDoesNotSatisfyAbstractClassGenericConstraint()
    {
        // Arrange
        var serviceType = typeof(IFakeOpenGenericService<>);
        var implementationType = typeof(ClassWithAbstractClassConstraint<>);
        var descriptor = new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient);
        var callSiteFactory = GetCallSiteFactory(descriptor);
        // Act
        var nonMatchingType = typeof(IFakeOpenGenericService<object>);
        // Assert
        Assert.Throws<ArgumentException>(() => callSiteFactory(nonMatchingType));
    }

    [Fact]
    public void CreateCallSite_ReturnsService_IfClosedTypeSatisfiesAbstractClassGenericConstraint()
    {
        // Arrange
        var serviceType = typeof(IFakeOpenGenericService<>);
        var implementationType = typeof(ClassWithAbstractClassConstraint<>);
        var descriptor = new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient);
        var callSiteFactory = GetCallSiteFactory(descriptor, new ServiceDescriptor(typeof(ClassInheritingAbstractClass), new ClassInheritingAbstractClass()));
        // Act
        var matchingType = typeof(IFakeOpenGenericService<ClassInheritingAbstractClass>);
        var matchingCallSite = callSiteFactory(matchingType);
        // Assert
        Assert.NotNull(matchingCallSite);
    }

    [Fact]
    public void CreateCallSite_Throws_IfClosedTypeDoesNotSatisfySelfReferencingConstraint()
    {
        // Arrange
        var serviceType = typeof(IFakeOpenGenericService<>);
        var implementationType = typeof(ClassWithSelfReferencingConstraint<>);
        var descriptor = new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient);
        var callSiteFactory = GetCallSiteFactory(descriptor);
        // Act
        var nonMatchingType = typeof(IFakeOpenGenericService<object>);
        // Assert
        Assert.Throws<ArgumentException>(() => callSiteFactory(nonMatchingType));
    }

    [Fact]
    public void CreateCallSite_Throws_IfComplexClosedTypeDoesNotSatisfySelfReferencingConstraint()
    {
        // Arrange
        var serviceType = typeof(IFakeOpenGenericService<>);
        var implementationType = typeof(ClassWithSelfReferencingConstraint<>);
        var descriptor = new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient);
        var callSiteFactory = GetCallSiteFactory(descriptor);
        // Act
        var nonMatchingType = typeof(IFakeOpenGenericService<int[]>);
        // Assert
        Assert.Throws<ArgumentException>(() => callSiteFactory(nonMatchingType));
    }

    [Fact]
    public void CreateCallSite_ReturnsService_IfClosedTypeSatisfiesSelfReferencing()
    {
        // Arrange
        var serviceType = typeof(IFakeOpenGenericService<>);
        var implementationType = typeof(ClassWithSelfReferencingConstraint<>);
        var descriptor = new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient);
        var callSiteFactory = GetCallSiteFactory(descriptor, new ServiceDescriptor(typeof(string), ""));
        // Act
        var matchingType = typeof(IFakeOpenGenericService<string>);
        var matchingCallSite = callSiteFactory(matchingType);
        // Assert
        Assert.NotNull(matchingCallSite);
    }

    [Fact]
    public void CreateCallSite_ReturnsEmpty_IfClosedTypeSatisfiesBaseClassConstraintButRegisteredTypeNotExactMatch()
    {
        // Arrange
        var classInheritingAbstractClassImplementationType = typeof(ClassWithAbstractClassConstraint<ClassInheritingAbstractClass>);
        var classInheritingAbstractClassDescriptor = new ServiceDescriptor(typeof(IFakeOpenGenericService<ClassInheritingAbstractClass>), classInheritingAbstractClassImplementationType, ServiceLifetime.Transient);
        var classAlsoInheritingAbstractClassImplementationType = typeof(ClassWithAbstractClassConstraint<ClassAlsoInheritingAbstractClass>);
        var classAlsoInheritingAbstractClassDescriptor = new ServiceDescriptor(typeof(IFakeOpenGenericService<ClassAlsoInheritingAbstractClass>), classAlsoInheritingAbstractClassImplementationType, ServiceLifetime.Transient);
        var classInheritingClassInheritingAbstractClassImplementationType = typeof(ClassWithAbstractClassConstraint<ClassInheritingClassInheritingAbstractClass>);
        var classInheritingClassInheritingAbstractClassDescriptor = new ServiceDescriptor(typeof(IFakeOpenGenericService<ClassInheritingClassInheritingAbstractClass>), classInheritingClassInheritingAbstractClassImplementationType, ServiceLifetime.Transient);
        var notMatchingServiceType = typeof(IFakeOpenGenericService<PocoClass>);
        var notMatchingType = typeof(FakeService);
        var notMatchingDescriptor = new ServiceDescriptor(notMatchingServiceType, notMatchingType, ServiceLifetime.Transient);

        var callSiteFactory = GetCallSiteFactory(classInheritingAbstractClassDescriptor, classAlsoInheritingAbstractClassDescriptor, classInheritingClassInheritingAbstractClassDescriptor, notMatchingDescriptor);
        // Act
        var matchingType = typeof(IEnumerable<IFakeOpenGenericService<AbstractClass>>);
        var matchingCallSite = callSiteFactory(matchingType);
        // Assert
        var enumerableCall = Assert.IsType<IEnumerableCallSite>(matchingCallSite);

        Assert.Empty(enumerableCall.ServiceCallSites);
    }

    [Fact]
    public void CreateCallSite_ReturnsMatchingTypes_IfClosedTypeSatisfiesBaseClassConstraintAndRegisteredType()
    {
        // Arrange
        var serviceType = typeof(IFakeOpenGenericService<AbstractClass>);
        var classInheritingAbstractClassImplementationType = typeof(ClassWithAbstractClassConstraint<ClassInheritingAbstractClass>);
        var classInheritingAbstractClassDescriptor = new ServiceDescriptor(serviceType, classInheritingAbstractClassImplementationType, ServiceLifetime.Transient);
        var classAlsoInheritingAbstractClassImplementationType = typeof(ClassWithAbstractClassConstraint<ClassAlsoInheritingAbstractClass>);
        var classAlsoInheritingAbstractClassDescriptor = new ServiceDescriptor(serviceType, classAlsoInheritingAbstractClassImplementationType, ServiceLifetime.Transient);
        var classInheritingClassInheritingAbstractClassImplementationType = typeof(ClassWithAbstractClassConstraint<ClassInheritingClassInheritingAbstractClass>);
        var classInheritingClassInheritingAbstractClassDescriptor = new ServiceDescriptor(serviceType, classInheritingClassInheritingAbstractClassImplementationType, ServiceLifetime.Transient);
        var notMatchingServiceType = typeof(IFakeOpenGenericService<PocoClass>);
        var notMatchingType = typeof(FakeService);
        var notMatchingDescriptor = new ServiceDescriptor(notMatchingServiceType, notMatchingType, ServiceLifetime.Transient);

        var descriptors = new[]
        {
            classInheritingAbstractClassDescriptor,
            new ServiceDescriptor(typeof(ClassInheritingAbstractClass), new ClassInheritingAbstractClass()),
            classAlsoInheritingAbstractClassDescriptor,
            new ServiceDescriptor(typeof(ClassAlsoInheritingAbstractClass), new ClassAlsoInheritingAbstractClass()),
            classInheritingClassInheritingAbstractClassDescriptor,
            new ServiceDescriptor(typeof(ClassInheritingClassInheritingAbstractClass), new ClassInheritingClassInheritingAbstractClass()),
            notMatchingDescriptor
        };
        var callSiteFactory = GetCallSiteFactory(descriptors);
        // Act
        var matchingType = typeof(IEnumerable<>).MakeGenericType(serviceType);
        var matchingCallSite = callSiteFactory(matchingType);
        // Assert
        var enumerableCall = Assert.IsType<IEnumerableCallSite>(matchingCallSite);

        var matchingTypes = new[]
        {
            classInheritingAbstractClassImplementationType,
            classAlsoInheritingAbstractClassImplementationType,
            classInheritingClassInheritingAbstractClassImplementationType
        };
        Assert.Equal(matchingTypes.Length, enumerableCall.ServiceCallSites.Length);
        Assert.Equal(matchingTypes, enumerableCall.ServiceCallSites.Select(scs => scs.ImplementationType).ToArray());
    }

    [Theory]
    [InlineData(typeof(IFakeOpenGenericService<int>), default(int), new[] { typeof(FakeOpenGenericService<int>), typeof(ClassWithStructConstraint<int>), typeof(ClassWithNewConstraint<int>), typeof(ClassWithSelfReferencingConstraint<int>) })]
    [InlineData(typeof(IFakeOpenGenericService<string>), "", new[] { typeof(FakeOpenGenericService<string>), typeof(ClassWithClassConstraint<string>), typeof(ClassWithInterfaceConstraint<string>), typeof(ClassWithSelfReferencingConstraint<string>) })]
    [InlineData(typeof(IFakeOpenGenericService<int[]>), new[] { 1, 2, 3 }, new[] { typeof(FakeOpenGenericService<int[]>), typeof(ClassWithClassConstraint<int[]>), typeof(ClassWithInterfaceConstraint<int[]>) })]
    public void CreateCallSite_ReturnsMatchingTypesThatMatchCorrectConstraints(Type closedServiceType, object value, Type[] matchingImplementationTypes)
    {
        // Arrange
        var serviceType = typeof(IFakeOpenGenericService<>);
        var noConstraintImplementationType = typeof(FakeOpenGenericService<>);
        var noConstraintDescriptor = new ServiceDescriptor(serviceType, noConstraintImplementationType, ServiceLifetime.Transient);
        var structImplementationType = typeof(ClassWithStructConstraint<>);
        var structDescriptor = new ServiceDescriptor(serviceType, structImplementationType, ServiceLifetime.Transient);
        var classImplementationType = typeof(ClassWithClassConstraint<>);
        var classDescriptor = new ServiceDescriptor(serviceType, classImplementationType, ServiceLifetime.Transient);
        var newImplementationType = typeof(ClassWithNewConstraint<>);
        var newDescriptor = new ServiceDescriptor(serviceType, newImplementationType, ServiceLifetime.Transient);
        var interfaceImplementationType = typeof(ClassWithInterfaceConstraint<>);
        var interfaceDescriptor = new ServiceDescriptor(serviceType, interfaceImplementationType, ServiceLifetime.Transient);
        var selfConstraintImplementationType = typeof(ClassWithSelfReferencingConstraint<>);
        var selfConstraintDescriptor = new ServiceDescriptor(serviceType, selfConstraintImplementationType, ServiceLifetime.Transient);
        var serviceValueType = closedServiceType.GenericTypeArguments[0];
        var serviceValueDescriptor = new ServiceDescriptor(serviceValueType, value);
        var callSiteFactory = GetCallSiteFactory(noConstraintDescriptor, structDescriptor, classDescriptor, newDescriptor, interfaceDescriptor, selfConstraintDescriptor, serviceValueDescriptor);
        var collectionType = typeof(IEnumerable<>).MakeGenericType(closedServiceType);
        // Act
        var callSite = callSiteFactory(collectionType);
        // Assert
        var enumerableCall = Assert.IsType<IEnumerableCallSite>(callSite);
        Assert.Equal(matchingImplementationTypes.Length, enumerableCall.ServiceCallSites.Length);
        Assert.Equal(matchingImplementationTypes, enumerableCall.ServiceCallSites.Select(scs => scs.ImplementationType).ToArray());
    }

    public static TheoryData CreateCallSite_PicksConstructorWithTheMostNumberOfResolvedParametersData =>
        new TheoryData<Type, Func<Type, ServiceCallSite>, Type[]>
        {
            {
                typeof(TypeWithSupersetConstructors),
                GetCallSiteFactory(
                    new ServiceDescriptor(typeof(TypeWithSupersetConstructors), typeof(TypeWithSupersetConstructors), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFakeService), new FakeService())
                ),
                [typeof(IFakeService)]
            },
            {
                typeof(TypeWithSupersetConstructors),
                GetCallSiteFactory(
                    new ServiceDescriptor(typeof(TypeWithSupersetConstructors), typeof(TypeWithSupersetConstructors), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFactoryService), new TransientFactoryService())
                ),
                [typeof(IFactoryService)]
            },
            {
                typeof(TypeWithSupersetConstructors),
                GetCallSiteFactory(
                    new ServiceDescriptor(typeof(TypeWithSupersetConstructors), typeof(TypeWithSupersetConstructors), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFakeService), new FakeService()),
                    new ServiceDescriptor(typeof(IFactoryService), new TransientFactoryService())
                ),
                [typeof(IFakeService), typeof(IFactoryService)]
            },
            {
                typeof(TypeWithSupersetConstructors),
                GetCallSiteFactory(
                    new ServiceDescriptor(typeof(TypeWithSupersetConstructors), typeof(TypeWithSupersetConstructors), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFakeMultipleService), new FakeService()),
                    new ServiceDescriptor(typeof(IFakeService), new FakeService()),
                    new ServiceDescriptor(typeof(IFactoryService), new TransientFactoryService())
                ),
                [typeof(IFakeService), typeof(IFakeMultipleService), typeof(IFactoryService)]
            },
            {
                typeof(TypeWithSupersetConstructors),
                GetCallSiteFactory(
                    new ServiceDescriptor(typeof(TypeWithSupersetConstructors), typeof(TypeWithSupersetConstructors), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFakeMultipleService), new FakeService()),
                    new ServiceDescriptor(typeof(IFakeService), new FakeService()),
                    new ServiceDescriptor(typeof(IFactoryService), new TransientFactoryService()),
                    new ServiceDescriptor(typeof(IFakeScopedService), new FakeService())
                ),
                [typeof(IFakeMultipleService), typeof(IFactoryService), typeof(IFakeService), typeof(IFakeScopedService)]
            },
            {
                typeof(TypeWithSupersetConstructors),
                GetCallSiteFactory(
                    new ServiceDescriptor(typeof(TypeWithSupersetConstructors), typeof(TypeWithSupersetConstructors), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFakeMultipleService), new FakeService()),
                    new ServiceDescriptor(typeof(IFakeService), new FakeService()),
                    new ServiceDescriptor(typeof(IFactoryService), new TransientFactoryService()),
                    new ServiceDescriptor(typeof(IFakeScopedService), new FakeService())
                ),
                [typeof(IFakeMultipleService), typeof(IFactoryService), typeof(IFakeService), typeof(IFakeScopedService)]
            },
            {
                typeof(TypeWithGenericServices),
                GetCallSiteFactory(
                    new ServiceDescriptor(typeof(TypeWithGenericServices), typeof(TypeWithGenericServices), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFakeService), typeof(FakeService), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFakeOpenGenericService<>), typeof(FakeOpenGenericService<>), ServiceLifetime.Transient)
                ),
                [typeof(IFakeService), typeof(IFakeOpenGenericService<IFakeService>)]
            },
            {
                typeof(TypeWithGenericServices),
                GetCallSiteFactory(
                    new ServiceDescriptor(typeof(TypeWithGenericServices), typeof(TypeWithGenericServices), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFakeService), typeof(FakeService), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFakeOpenGenericService<>), typeof(FakeOpenGenericService<>), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFactoryService), typeof(TransientFactoryService), ServiceLifetime.Transient)
                ),
                [typeof(IFakeService), typeof(IFactoryService), typeof(IFakeOpenGenericService<IFakeService>)]
            }
        };

    [Theory]
    [MemberData(nameof(CreateCallSite_PicksConstructorWithTheMostNumberOfResolvedParametersData))]
    private void CreateCallSite_PicksConstructorWithTheMostNumberOfResolvedParameters(
        Type type,
        Func<Type, ServiceCallSite> callSiteFactory,
        Type[] expectedConstructorParameters)
    {
        // Act
        var callSite = callSiteFactory(type);

        // Assert
        Assert.Equal(CallSiteResultCacheLocation.Dispose, callSite.Cache.Location);
        var constructorCallSite = Assert.IsType<ConstructorCallSite>(callSite);
        Assert.Equal(expectedConstructorParameters, GetParameters(constructorCallSite));
    }

    public static TheoryData CreateCallSite_ConsidersConstructorsWithDefaultValuesData =>
        new TheoryData<Func<Type, object>, Type[]>
        {
            {
                GetCallSiteFactory(
                    new ServiceDescriptor(typeof(TypeWithDefaultConstructorParameters), typeof(TypeWithDefaultConstructorParameters), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFakeMultipleService), typeof(FakeService), ServiceLifetime.Transient)
                ),
                [typeof(IFakeMultipleService), typeof(IFakeService)]
            },
            {
                GetCallSiteFactory(
                    new ServiceDescriptor(typeof(TypeWithDefaultConstructorParameters), typeof(TypeWithDefaultConstructorParameters), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFactoryService), typeof(TransientFactoryService), ServiceLifetime.Transient)
                ),
                [typeof(IFactoryService), typeof(IFakeScopedService)]
            },
            {
                GetCallSiteFactory(
                    new ServiceDescriptor(typeof(TypeWithDefaultConstructorParameters), typeof(TypeWithDefaultConstructorParameters), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFakeScopedService), typeof(FakeService), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFactoryService), typeof(TransientFactoryService), ServiceLifetime.Transient)
                ),
                [typeof(IFactoryService), typeof(IFakeScopedService)]
            }
        };

    [Theory]
    [MemberData(nameof(CreateCallSite_ConsidersConstructorsWithDefaultValuesData))]
    private void CreateCallSite_ConsidersConstructorsWithDefaultValues(
        Func<Type, ServiceCallSite> callSiteFactory,
        Type[] expectedConstructorParameters)
    {
        // Arrange
        var type = typeof(TypeWithDefaultConstructorParameters);

        // Act
        var callSite = callSiteFactory(type);

        // Assert
        Assert.Equal(CallSiteResultCacheLocation.Dispose, callSite.Cache.Location);
        var constructorCallSite = Assert.IsType<ConstructorCallSite>(callSite);
        Assert.Equal(expectedConstructorParameters, GetParameters(constructorCallSite));
    }

    [Fact]
    public void CreateCallSite_ThrowsIfTypeHasSingleConstructorWithUnresolvableParameters()
    {
        // Arrange
        var type = typeof(TypeWithParameterizedConstructor);
        var descriptor = new ServiceDescriptor(type, type, ServiceLifetime.Transient);

        var callSiteFactory = GetCallSiteFactory(descriptor);

        // Act and Assert
        var ex = Assert.Throws<InvalidOperationException>(
            () => callSiteFactory(type));
        Assert.Equal($"Unable to resolve service for type '{typeof(IFakeService)}' while attempting to activate '{type}'.",
            ex.Message);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void GetSlotTests(int numberOfServices)
    {
        var serviceDescriptors = new[] {
            ServiceDescriptor.Singleton<ICustomService, CustomService1>(),
            ServiceDescriptor.Singleton<ICustomService, CustomService2>(),
            ServiceDescriptor.Singleton<ICustomService, CustomService3>(),
            ServiceDescriptor.Singleton<ICustomService, CustomService4>(),
            ServiceDescriptor.Singleton<ICustomService, CustomService5>()
        };

        var callsiteFactory = new CallSiteFactory(serviceDescriptors.Take(numberOfServices).ToArray());

        for (var i = 0; i < numberOfServices; i++)
        {
            Assert.Equal(numberOfServices - i - 1, callsiteFactory.GetSlot(serviceDescriptors[i]));
        }
    }

    interface ICustomService
    {

    }

    class CustomService1 : ICustomService { }
    class CustomService2 : ICustomService { }
    class CustomService3 : ICustomService { }
    class CustomService4 : ICustomService { }
    class CustomService5 : ICustomService { }

    [Theory]
    [InlineData(typeof(TypeWithMultipleParameterizedConstructors))]
    [InlineData(typeof(TypeWithSupersetConstructors))]
    public void CreateCallSite_ThrowsIfTypeHasNoConstructurWithResolvableParameters(Type type)
    {
        // Arrange
        var descriptor = new ServiceDescriptor(type, type, ServiceLifetime.Transient);
        var callSiteFactory = GetCallSiteFactory(
            descriptor,
            new ServiceDescriptor(typeof(IFakeMultipleService), typeof(FakeService), ServiceLifetime.Transient),
            new ServiceDescriptor(typeof(IFakeScopedService), typeof(FakeService), ServiceLifetime.Transient)
        );

        // Act and Assert
        var ex = Assert.Throws<InvalidOperationException>(
            () => callSiteFactory(type));
        Assert.Equal($"No constructor for type '{type}' can be instantiated using services from the service container and default values.",
            ex.Message);
    }

    public static TheoryData CreateCallSite_ThrowsIfMultipleNonOverlappingConstructorsCanBeResolvedData =>
        new TheoryData<Type, Func<Type, object>, Type[][]>
        {
            {
                typeof(TypeWithDefaultConstructorParameters),
                GetCallSiteFactory(
                    new ServiceDescriptor(typeof(TypeWithDefaultConstructorParameters), typeof(TypeWithDefaultConstructorParameters), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFactoryService), typeof(TransientFactoryService), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFakeMultipleService), typeof(FakeService), ServiceLifetime.Transient)
                ),
                [
                    [typeof(IFakeMultipleService), typeof(IFakeService)],
                    [typeof(IFactoryService), typeof(IFakeScopedService)]
                ]
            },
            {
                typeof(TypeWithMultipleParameterizedConstructors),
                GetCallSiteFactory(
                    new ServiceDescriptor(typeof(TypeWithMultipleParameterizedConstructors), typeof(TypeWithMultipleParameterizedConstructors), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFakeService), typeof(FakeService), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFactoryService), typeof(TransientFactoryService), ServiceLifetime.Transient)
                ),
                [
                    [typeof(IFactoryService)],
                    [typeof(IFakeService)]
                ]
            },
            {
                typeof(TypeWithNonOverlappedConstructors),
                GetCallSiteFactory(
                    new ServiceDescriptor(typeof(TypeWithNonOverlappedConstructors), typeof(TypeWithNonOverlappedConstructors), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFakeScopedService), typeof(FakeService), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFakeMultipleService), typeof(FakeService), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFakeOuterService), typeof(FakeOuterService), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFakeService), typeof(FakeService), ServiceLifetime.Transient)
                ),
                [
                    [typeof(IFakeScopedService), typeof(IFakeService), typeof(IFakeMultipleService)],
                    [typeof(IFakeOuterService)]
                ]
            },
            {
                typeof(TypeWithUnresolvableEnumerableConstructors),
                GetCallSiteFactory(
                    new ServiceDescriptor(typeof(TypeWithUnresolvableEnumerableConstructors), typeof(TypeWithUnresolvableEnumerableConstructors), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFakeService), typeof(FakeService), ServiceLifetime.Transient)
                ),
                [
                    [typeof(IFakeService)],
                    [typeof(IEnumerable<IFakeService>)]
                ]
            },
            {
                typeof(TypeWithUnresolvableEnumerableConstructors),
                GetCallSiteFactory(
                    new ServiceDescriptor(typeof(TypeWithUnresolvableEnumerableConstructors), typeof(TypeWithUnresolvableEnumerableConstructors), ServiceLifetime.Transient),
                    new ServiceDescriptor(typeof(IFactoryService), typeof(TransientFactoryService), ServiceLifetime.Transient)
                ),
                [
                    [typeof(IEnumerable<IFakeService>)],
                    [typeof(IFactoryService)]
                ]
            },
        };

    [Theory]
    [MemberData(nameof(CreateCallSite_ThrowsIfMultipleNonOverlappingConstructorsCanBeResolvedData))]
    public void CreateCallSite_ThrowsIfMultipleNonOverlappingConstructorsCanBeResolved(
        Type type,
        Func<Type, object> callSiteFactory,
        Type[][] expectedConstructorParameterTypes)
    {
        // Arrange
        var expectedMessage =
            string.Join(
                Environment.NewLine,
                $"Unable to activate type '{type}'. The following constructors are ambiguous:",
                GetConstructor(type, expectedConstructorParameterTypes[0]),
                GetConstructor(type, expectedConstructorParameterTypes[1]));

        // Act and Assert
        var ex = Assert.Throws<InvalidOperationException>(
            () => callSiteFactory(type));
        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void CreateCallSite_ThrowsIfMultipleNonOverlappingConstructorsForGenericTypesCanBeResolved()
    {
        // Arrange
        var type = typeof(TypeWithGenericServices);
        var expectedMessage = $"Unable to activate type '{type}'. The following constructors are ambiguous:";

        var callSiteFactory = GetCallSiteFactory(
            new ServiceDescriptor(type, type, ServiceLifetime.Transient),
            new ServiceDescriptor(typeof(IFakeService), typeof(FakeService), ServiceLifetime.Transient),
            new ServiceDescriptor(typeof(IFakeMultipleService), typeof(FakeService), ServiceLifetime.Transient),
            new ServiceDescriptor(typeof(IFakeOpenGenericService<>), typeof(FakeOpenGenericService<>), ServiceLifetime.Transient)
        );

        // Act and Assert
        var ex = Assert.Throws<InvalidOperationException>(
            () => callSiteFactory(type));
        Assert.StartsWith(expectedMessage, ex.Message);
    }

    public static TheoryData<ServiceDescriptor[], object> EnumberableCachedAtLowestLevelData = new()
    {
        {
            [
                new ServiceDescriptor(typeof(FakeService), typeof(FakeService), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(FakeService), typeof(FakeService), ServiceLifetime.Singleton),
                new ServiceDescriptor(typeof(FakeService), typeof(FakeService), ServiceLifetime.Scoped)
            ],
            CallSiteResultCacheLocation.None
        },
        {
            [
                new ServiceDescriptor(typeof(FakeService), typeof(FakeService), ServiceLifetime.Scoped),
                new ServiceDescriptor(typeof(FakeService), typeof(FakeService), ServiceLifetime.Singleton),
                new ServiceDescriptor(typeof(FakeService), typeof(FakeService), ServiceLifetime.Scoped)
            ],
            CallSiteResultCacheLocation.Scope
        },
        {
            [
                new ServiceDescriptor(typeof(FakeService), typeof(FakeService), ServiceLifetime.Singleton),
                new ServiceDescriptor(typeof(FakeService), typeof(FakeService), ServiceLifetime.Singleton),
                new ServiceDescriptor(typeof(FakeService), typeof(FakeService), ServiceLifetime.Singleton)
            ],
            CallSiteResultCacheLocation.Root
        }
    };

    [Theory]
    [MemberData(nameof(EnumberableCachedAtLowestLevelData))]
    public void CreateCallSite_EnumberableCachedAtLowestLevel(ServiceDescriptor[] descriptors, object expectedCacheLocation)
    {
        var factory = GetCallSiteFactory(descriptors);
        var callSite = factory(typeof(IEnumerable<FakeService>));

        var expectedLocation = (CallSiteResultCacheLocation)expectedCacheLocation;

        Assert.Equal(expectedLocation, callSite.Cache.Location);
        Assert.Equal(0, callSite.Cache.Key.Slot);
        Assert.Equal(typeof(IEnumerable<FakeService>), callSite.Cache.Key.ServiceIdentifier.ServiceType);
    }

    [Fact]
    public void CallSitesAreUniquePerServiceTypeAndSlot()
    {
        // Connected graph
        // Class1 -> Class2 -> Class3
        // Class4 -> Class3
        // Class5 -> Class2 -> Class3
        var types = new[] { typeof(Class1), typeof(Class2), typeof(Class3), typeof(Class4), typeof(Class5) };

        for (var i = 0; i < 100; i++)
        {
            var factory = GetCallSiteFactory(types.Select(t => ServiceDescriptor.Transient(t, t)).ToArray());

            var tasks = new Task<ServiceCallSite>[types.Length];
            for (var j = 0; j < types.Length; j++)
            {
                var type = types[j];
                tasks[j] = Task.Run(() => factory(type));
            }

            Task.WaitAll(tasks);

            var callsites = tasks.Select(t => t.Result).Cast<ConstructorCallSite>().ToArray();

            Assert.Equal(5, callsites.Length);
            // Class1 -> Class2
            Assert.Same(callsites[0].ParameterCallSites[0], callsites[1]);
            // Class2 -> Class3
            Assert.Same(callsites[1].ParameterCallSites[0], callsites[2]);
            // Class4 -> Class3
            Assert.Same(callsites[3].ParameterCallSites[0], callsites[2]);
            // Class5 -> Class2
            Assert.Same(callsites[4].ParameterCallSites[0], callsites[1]);
        }
    }

    [Fact]
    public void CallSitesAreUniquePerServiceTypeAndSlotWithOpenGenericInGraph()
    {
        // Connected graph
        // ClassA -> ClassB -> ClassC<object>
        // ClassD -> ClassC<string>
        // ClassE -> ClassB -> ClassC<object>
        var types = new[] { typeof(ClassA), typeof(ClassB), typeof(ClassC<>), typeof(ClassD), typeof(ClassE) };

        for (var i = 0; i < 100; i++)
        {
            var factory = GetCallSiteFactory(types.Select(t => ServiceDescriptor.Transient(t, t)).ToArray());

            var tasks = new Task<ServiceCallSite>[types.Length];
            for (var j = 0; j < types.Length; j++)
            {
                var type = types[j];
                tasks[j] = Task.Run(() => factory(type));
            }

            Task.WaitAll(tasks);

            var callsites = tasks.Select(t => t.Result).Cast<ConstructorCallSite>().ToArray();

            var cOfObject = factory(typeof(ClassC<object>));
            var cOfString = factory(typeof(ClassC<string>));

            Assert.Equal(5, callsites.Length);
            // ClassA -> ClassB
            Assert.Same(callsites[0].ParameterCallSites[0], callsites[1]);
            // ClassB -> ClassC<object>
            Assert.Same(callsites[1].ParameterCallSites[0], cOfObject);
            // ClassD -> ClassC<string>
            Assert.Same(callsites[3].ParameterCallSites[0], cOfString);
            // ClassE -> ClassB
            Assert.Same(callsites[4].ParameterCallSites[0], callsites[1]);
        }
    }

    [ConditionalFact(typeof(RemoteExecutor), nameof(RemoteExecutor.IsSupported))]
    [SkipOnTargetFramework(TargetFrameworkMonikers.NetFramework)] // RuntimeConfigurationOptions are not supported on .NET Framework (and neither is trimming)
    public void VerifyOpenGenericTrimmabilityChecks()
    {
        var options = new RemoteInvokeOptions();
        options.RuntimeConfigurationOptions.Add("Microsoft.Extensions.DependencyInjection.VerifyOpenGenericServiceTrimmability", "true");

        using var remoteHandle = RemoteExecutor.Invoke(() =>
        {
            (Type, Type)[] invalidTestCases =
            [
                (typeof(IFakeOpenGenericService<>), typeof(ClassWithNewConstraint<>)),
                (typeof(IServiceWithoutTrimmingAnnotations<>), typeof(ServiceWithTrimmingAnnotations<>)),
                (typeof(IServiceWithPublicConstructors<>), typeof(ServiceWithPublicProperties<>)),
                (typeof(IServiceWithTwoGenerics<,>), typeof(ServiceWithTwoGenericsInvalid<,>))
            ];
            foreach ((var serviceType, var implementationType) in invalidTestCases)
            {
                ServiceDescriptor[] serviceDescriptors =
                [
                    new(serviceType, implementationType, ServiceLifetime.Singleton)
                ];

                Assert.Throws<ArgumentException>(() => new CallSiteFactory(serviceDescriptors));
            }

            (Type, Type)[] validTestCases =
            [
                (typeof(IFakeOpenGenericService<>), typeof(FakeOpenGenericService<>)),
                (typeof(IServiceWithPublicConstructors<>), typeof(ServiceWithPublicConstructors<>)),
                (typeof(IServiceWithTwoGenerics<,>), typeof(ServiceWithTwoGenericsValid<,>)),
                (typeof(IServiceWithMoreMemberTypes<>), typeof(ServiceWithLessMemberTypes<>))
            ];
            foreach ((var serviceType, var implementationType) in validTestCases)
            {
                ServiceDescriptor[] serviceDescriptors =
                [
                    new(serviceType, implementationType, ServiceLifetime.Singleton)
                ];

                Assert.NotNull(new CallSiteFactory(serviceDescriptors));
            }
        }, options);
    }

    [ConditionalFact(typeof(RemoteExecutor), nameof(RemoteExecutor.IsSupported))]
    [SkipOnTargetFramework(TargetFrameworkMonikers.NetFramework)] // RuntimeConfigurationOptions are not supported on .NET Framework (and neither is NativeAOT)
    public void VerifyDynamicCodeNotSupportedChecks()
    {
        Func<Type, ServiceCallSite> CreateAotCompatibilityCallSiteFactory()
        {
            ServiceDescriptor[] descriptors =
            [
                new(typeof(IFakeOpenGenericService<>), typeof(ClassWithNoConstraints<>), ServiceLifetime.Transient),
                new(typeof(IServiceWithTwoGenerics<,>), typeof(ServiceWithTwoGenericsValid<,>), ServiceLifetime.Transient),

                new(typeof(Struct1), new Struct1(1)),
                new(typeof(Struct1), new Struct1(2))
            ];

            return GetCallSiteFactory(descriptors);
        }

        var options = new RemoteInvokeOptions();
        options.RuntimeConfigurationOptions.Add("System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported", "false");
        options.RuntimeConfigurationOptions.Add("System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeCompiled", "false");

        using var remoteHandle = RemoteExecutor.Invoke(() =>
        {
            Func<Type, ServiceCallSite> callSiteFactory = CreateAotCompatibilityCallSiteFactory();

            // Verify open generics throw when passing ValueTypes
            Assert.Throws<InvalidOperationException>(() => callSiteFactory(typeof(IFakeOpenGenericService<Struct1>)));
            Assert.Throws<InvalidOperationException>(() => callSiteFactory(typeof(IFakeOpenGenericService<int>)));
            Assert.Throws<InvalidOperationException>(() => callSiteFactory(typeof(IServiceWithTwoGenerics<Class3, int>)));
            Assert.Throws<InvalidOperationException>(() => callSiteFactory(typeof(IServiceWithTwoGenerics<int, Class3>)));

            var callSite = callSiteFactory(typeof(IFakeOpenGenericService<Class3>));
            Assert.Equal(CallSiteKind.Constructor, callSite.Kind);
            Assert.Equal(typeof(ClassWithNoConstraints<Class3>), callSite.ImplementationType);

            callSite = callSiteFactory(typeof(IServiceWithTwoGenerics<Class3, Class3>));
            Assert.Equal(CallSiteKind.Constructor, callSite.Kind);
            Assert.Equal(typeof(ServiceWithTwoGenericsValid<Class3, Class3>), callSite.ImplementationType);

            // Verify Enumerable services throw when passing ValueTypes
            Assert.Throws<InvalidOperationException>(() => callSiteFactory(typeof(IEnumerable<Struct1>)));

            callSite = callSiteFactory(typeof(Struct1));
            Assert.Equal(CallSiteKind.Constant, callSite.Kind);
            Assert.Equal(2, ((Struct1)callSite.Value).Value);
        }, options);

        // Verify the above scenarios work when IsDynamicCodeSupported + IsDynamicCodeCompiled are not set
        Func<Type, ServiceCallSite> callSiteFactory = CreateAotCompatibilityCallSiteFactory();

        // Open Generics
        var callSite = callSiteFactory(typeof(IFakeOpenGenericService<Struct1>));
        Assert.Equal(CallSiteKind.Constructor, callSite.Kind);
        Assert.Equal(typeof(ClassWithNoConstraints<Struct1>), callSite.ImplementationType);

        callSite = callSiteFactory(typeof(IFakeOpenGenericService<int>));
        Assert.Equal(CallSiteKind.Constructor, callSite.Kind);
        Assert.Equal(typeof(ClassWithNoConstraints<int>), callSite.ImplementationType);

        callSite = callSiteFactory(typeof(IServiceWithTwoGenerics<Class3, int>));
        Assert.Equal(CallSiteKind.Constructor, callSite.Kind);
        Assert.Equal(typeof(ServiceWithTwoGenericsValid<Class3, int>), callSite.ImplementationType);

        callSite = callSiteFactory(typeof(IServiceWithTwoGenerics<int, Class3>));
        Assert.Equal(CallSiteKind.Constructor, callSite.Kind);
        Assert.Equal(typeof(ServiceWithTwoGenericsValid<int, Class3>), callSite.ImplementationType);

        // Enumerable 
        callSite = callSiteFactory(typeof(IEnumerable<Struct1>));
        Assert.Equal(CallSiteKind.IEnumerable, callSite.Kind);
        var enumerableCallSite = (IEnumerableCallSite)callSite;
        Assert.Equal(2, enumerableCallSite.ServiceCallSites.Length);
        Assert.Equal(CallSiteKind.Constant, enumerableCallSite.ServiceCallSites[0].Kind);
        Assert.Equal(1, ((Struct1)enumerableCallSite.ServiceCallSites[0].Value).Value);
        Assert.Equal(CallSiteKind.Constant, enumerableCallSite.ServiceCallSites[1].Kind);
        Assert.Equal(2, ((Struct1)enumerableCallSite.ServiceCallSites[1].Value).Value);
    }

    private static Func<Type, ServiceCallSite> GetCallSiteFactory(params ServiceDescriptor[] descriptors)
    {
        var collection = new ServiceCollection();
        foreach (var descriptor in descriptors)
        {
            collection.Add(descriptor);
        }

        var callSiteFactory = new CallSiteFactory(collection.ToArray());

        return type => callSiteFactory.GetCallSite(ServiceIdentifier.FromServiceType(type), new CallSiteChain());
    }

    private static IEnumerable<Type> GetParameters(ConstructorCallSite constructorCallSite) =>
        constructorCallSite
            .ConstructorInfo
            .GetParameters()
            .Select(p => p.ParameterType);

    private static ConstructorInfo GetConstructor(Type type, Type[] parameterTypes) =>
        type.GetTypeInfo().DeclaredConstructors.First(
            c => c.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes));

    private class Class1 { public Class1(Class2 c2) { } }
    private class Class2 { public Class2(Class3 c3) { } }
    private class Class3 { }
    private class Class4 { public Class4(Class3 c3) { } }
    private class Class5 { public Class5(Class2 c2) { } }

    private record struct Struct1(int Value) { }

    // Open generic
    private class ClassA { public ClassA(ClassB cb) { } }
    private class ClassB { public ClassB(ClassC<object> cc) { } }
    private class ClassC<T> { }
    private class ClassD { public ClassD(ClassC<string> cd) { } }
    private class ClassE { public ClassE(ClassB cb) { } }

    // Open generic with trimming annotations
    private interface IServiceWithoutTrimmingAnnotations<T> { }
    private class ServiceWithTrimmingAnnotations<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T> : IServiceWithoutTrimmingAnnotations<T> { }

    private interface IServiceWithPublicConstructors<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T> { }
    private class ServiceWithPublicProperties<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : IServiceWithPublicConstructors<T> { }
    private class ServiceWithPublicConstructors<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T> : IServiceWithPublicConstructors<T> { }

    private interface IServiceWithTwoGenerics<T1, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T2> { }
    private class ServiceWithTwoGenericsInvalid<T1, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T2> : IServiceWithTwoGenerics<T1, T2> { }
    private class ServiceWithTwoGenericsValid<T1, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T2> : IServiceWithTwoGenerics<T1, T2> { }

    private interface IServiceWithMoreMemberTypes<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] T> { }
    private class ServiceWithLessMemberTypes<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : IServiceWithMoreMemberTypes<T> { }
}