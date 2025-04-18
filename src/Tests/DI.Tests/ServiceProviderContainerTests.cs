// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection.Specification;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using MicrosoftCopy.DependencyInjection.Tests.Fakes;
using MicrosoftCopy.DependencyInjection.Tests.Shared;
using RonSijm.Syringe;
using Xunit;

namespace MicrosoftCopy.DependencyInjection.Tests;

public abstract class ServiceProviderContainerTests : DependencyInjectionSpecificationTests
{
    [Fact]
    public void RethrowOriginalExceptionFromConstructor()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<ClassWithThrowingEmptyCtor>();
        serviceCollection.AddTransient<ClassWithThrowingCtor>();
        serviceCollection.AddTransient<IFakeService, FakeService>();

        var provider = CreateServiceProvider(serviceCollection);

        var ex1 = Assert.Throws<Exception>(() => provider.GetService<ClassWithThrowingEmptyCtor>());
        Assert.Equal(nameof(ClassWithThrowingEmptyCtor), ex1.Message);

        var ex2 = Assert.Throws<Exception>(() => provider.GetService<ClassWithThrowingCtor>());
        Assert.Equal(nameof(ClassWithThrowingCtor), ex2.Message);
    }

    [Fact]
    public void DependencyWithPrivateConstructorIsIdentifiedAsPartOfException()
    {
        // Arrange
        var expectedMessage = $"A suitable constructor for type '{typeof(ClassWithPrivateCtor).FullName}' could not be located. "
                              + "Ensure the type is concrete and services are registered for all parameters of a public constructor.";
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<ClassWithPrivateCtor>();
        serviceCollection.AddTransient<ClassDependsOnPrivateConstructorClass>();
        var serviceProvider = CreateServiceProvider(serviceCollection);

        // Act and Assert
        var ex = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetServices<ClassDependsOnPrivateConstructorClass>());
        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void AttemptingToResolveNonexistentServiceIndirectlyThrows()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddTransient<DependOnNonexistentService>();
        var provider = CreateServiceProvider(collection);

        // Act and Assert
        var ex = Assert.Throws<InvalidOperationException>(() => provider.GetService<DependOnNonexistentService>());
        Assert.Equal($"Unable to resolve service for type '{typeof(IFakeService)}' while attempting to activate " +
                     $"'{typeof(DependOnNonexistentService)}'.", ex.Message);
    }

    [Fact]
    public void AttemptingToIEnumerableResolveNonexistentServiceIndirectlyThrows()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddTransient<DependOnNonexistentService>();
        var provider = CreateServiceProvider(collection);

        // Act and Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            provider.GetService<IEnumerable<DependOnNonexistentService>>());
        Assert.Equal($"Unable to resolve service for type '{typeof(IFakeService)}' while attempting to activate " +
                     $"'{typeof(DependOnNonexistentService)}'.", ex.Message);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void MultipleServicesAreOrdered(int numberOfServices)
    {
        // Arrange
        var collection = new ServiceCollection();

        var serviceDescriptors = new[] {
            ServiceDescriptor.Singleton<ICustomService, CustomService1>(),
            ServiceDescriptor.Singleton<ICustomService, CustomService2>(),
            ServiceDescriptor.Singleton<ICustomService, CustomService3>(),
            ServiceDescriptor.Singleton<ICustomService, CustomService4>(),
            ServiceDescriptor.Singleton<ICustomService, CustomService5>(),
            ServiceDescriptor.Singleton<ICustomService, CustomService6>()
        };

        var serviceTypes = new[]
        {
            typeof(CustomService1),
            typeof(CustomService2),
            typeof(CustomService3),
            typeof(CustomService4),
            typeof(CustomService5),
            typeof(CustomService6),
        };

        foreach (var sd in serviceDescriptors.Take(numberOfServices))
        {
            collection.Add(sd);
        }

        var provider = collection.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true
        });

        // Act and Assert
        var customServices = provider.GetService<IEnumerable<ICustomService>>().ToArray();

        Assert.Equal(numberOfServices, customServices.Length);

        for (var i = 0; i < numberOfServices; i++)
        {
            Assert.IsAssignableFrom(serviceTypes[i], customServices[i]);
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
    class CustomService6 : ICustomService { }

    [Theory]
    // GenericTypeDefinition, Abstract GenericTypeDefinition
    [InlineData(typeof(IFakeOpenGenericService<>), typeof(AbstractFakeOpenGenericService<>))]
    // GenericTypeDefinition, Interface GenericTypeDefinition
    [InlineData(typeof(ICollection<>), typeof(IList<>))]
    // Implementation type is GenericTypeDefinition
    [InlineData(typeof(IList<int>), typeof(List<>))]
    // Implementation type is Abstract
    [InlineData(typeof(IFakeService), typeof(AbstractClass))]
    // Implementation type is Interface
    [InlineData(typeof(IFakeEveryService), typeof(IFakeService))]
    public void CreatingServiceProviderWithUnresolvableTypesThrows(Type serviceType, Type implementationType)
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient(serviceType, implementationType);

        // Act and Assert
        var exception = Assert.Throws<ArgumentException>(() => CreateServiceProvider(serviceCollection));
        Assert.Equal(
            $"Cannot instantiate implementation type '{implementationType}' for service type '{serviceType}'.",
            exception.Message);
    }

    [Theory]
    [MemberData(nameof(FailedOpenGenericTypeTestData))]
    public void CreatingServiceProviderWithUnresolvableOpenGenericTypesThrows(Type serviceType, Type implementationType, string errorMessage)
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient(serviceType, implementationType);

        // Act and Assert
        var exception = Assert.Throws<ArgumentException>(() => CreateServiceProvider(serviceCollection));
        Assert.StartsWith(errorMessage, exception.Message);
    }

    public static IEnumerable<object[]> FailedOpenGenericTypeTestData
    {
        get
        {
            var serviceType = typeof(IFakeOpenGenericService<>);
            // Service type is GenericTypeDefinition, implementation type is ConstructedGenericType
            yield return [serviceType, typeof(ClassWithNoConstraints<string>), $"Open generic service type '{serviceType}' requires registering an open generic implementation type."];
            // Service type is GenericTypeDefinition, implementation type has different generic type definition arity
            yield return [serviceType, typeof(FakeOpenGenericServiceWithTwoTypeArguments<,>), $"Arity of open generic service type '{serviceType}' does not equal arity of open generic implementation type '{typeof(FakeOpenGenericServiceWithTwoTypeArguments<,>)}'."];
        }
    }

    [Fact]
    public void DoesNotDisposeSingletonInstances()
    {
        var disposable = new Disposable();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(disposable);

        var provider = CreateServiceProvider(serviceCollection);
        provider.GetService<Disposable>();

        ((IDisposable)provider).Dispose();

        Assert.False(disposable.Disposed);
    }

    [Fact]
    public void ResolvesServiceMixedServiceAndOptionalStructConstructorArguments()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IFakeService, FakeService>();
        serviceCollection.AddSingleton<ClassWithServiceAndOptionalArgsCtorWithStructs>();

        var provider = CreateServiceProvider(serviceCollection);
        var service = provider.GetService<ClassWithServiceAndOptionalArgsCtorWithStructs>();
        Assert.NotNull(service);
    }

    [Fact]
    public void ResolvesServiceMixedServiceAndOptionalStructConstructorArgumentsReliably()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IFakeService, FakeService>();
        serviceCollection.AddTransient<ClassWithServiceAndOptionalArgsCtorWithStructs>();

        var provider = CreateServiceProvider(serviceCollection);

        // Repeatedly resolve and re-check to ensure dynamically generated code properly initializes the values types.
        for (var i = 0; i < 100; i++)
        {
            var service = provider.GetService<ClassWithServiceAndOptionalArgsCtorWithStructs>();

            Assert.NotNull(service);
            Assert.Equal(new DateTime(), service.DateTime);
            Assert.Equal(default(DateTime), service.DateTimeDefault);
            Assert.Equal(new TimeSpan(), service.TimeSpan);
            Assert.Equal(default(TimeSpan), service.TimeSpanDefault);
            Assert.Equal(new DateTimeOffset(), service.DateTimeOffset);
            Assert.Equal(default(DateTimeOffset), service.DateTimeOffsetDefault);
            Assert.Equal(new Guid(), service.Guid);
            Assert.Equal(default(Guid), service.GuidDefault);
            Assert.Equal(new ClassWithServiceAndOptionalArgsCtorWithStructs.CustomStruct(), service.CustomStructValue);
            Assert.Equal(default(ClassWithServiceAndOptionalArgsCtorWithStructs.CustomStruct), service.CustomStructDefault);
        }
    }

    public enum TheEnum
    {
        HelloWorld = -1,
        NiceWorld = 0,
        GoodByeWorld = 1,
    }

    [Theory]
    [InlineData(ServiceLifetime.Transient)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Singleton)]
    public void ResolvesConstantValueTypeServicesCorrectly(ServiceLifetime lifetime)
    {
        var serviceCollection = new ServiceCollection();
        if (lifetime == ServiceLifetime.Transient)
        {
            serviceCollection.AddTransient(typeof(int), _ => 4);
            serviceCollection.AddTransient(typeof(DateTime), _ => new DateTime());
            serviceCollection.AddTransient(typeof(TheEnum), _ => TheEnum.HelloWorld);

            serviceCollection.AddTransient(typeof(TimeSpan), _ => TimeSpan.Zero);
            serviceCollection.AddTransient(typeof(TimeSpan), _ => new TimeSpan(1, 2, 3));
        }
        else if (lifetime == ServiceLifetime.Scoped)
        {
            serviceCollection.AddScoped(typeof(int), _ => 4);
            serviceCollection.AddScoped(typeof(DateTime), _ => new DateTime());
            serviceCollection.AddScoped(typeof(TheEnum), _ => TheEnum.HelloWorld);

            serviceCollection.AddScoped(typeof(TimeSpan), _ => TimeSpan.Zero);
            serviceCollection.AddScoped(typeof(TimeSpan), _ => new TimeSpan(1, 2, 3));
        }
        else if (lifetime == ServiceLifetime.Singleton)
        {
            serviceCollection.AddSingleton(typeof(int), 4);
            serviceCollection.AddSingleton(typeof(DateTime), new DateTime());
            serviceCollection.AddSingleton(typeof(TheEnum), TheEnum.HelloWorld);

            serviceCollection.AddSingleton(typeof(TimeSpan), TimeSpan.Zero);
            serviceCollection.AddSingleton(typeof(TimeSpan), _ => new TimeSpan(1, 2, 3));
        }

        var provider = CreateServiceProvider(serviceCollection);

        var i = provider.GetService<int>();
        Assert.Equal(4, i);

        var d = provider.GetService<DateTime>();
        Assert.Equal(new DateTime(), d);

        var e = provider.GetService<TheEnum>();
        Assert.Equal(TheEnum.HelloWorld, e);

        var times = provider.GetServices<TimeSpan>();
        Assert.Equal([TimeSpan.Zero, new TimeSpan(1, 2, 3)], times);
    }

    [Fact]
    public void RootProviderDispose_PreventsServiceResolution()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IFakeService, FakeService>();

        var provider = CreateServiceProvider(serviceCollection);
        ((IDisposable)provider).Dispose();

        Assert.Throws<ObjectDisposedException>(() => provider.GetService<IFakeService>());
    }

    [Fact]
    public void RootProviderDispose_PreventsScopeCreation()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IFakeService, FakeService>();

        var provider = CreateServiceProvider(serviceCollection);
        ((IDisposable)provider).Dispose();

        Assert.Throws<ObjectDisposedException>(() => provider.CreateScope());
    }

    [Fact]
    public void RootProviderDispose_PreventsServiceResolution_InChildScope()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IFakeService, FakeService>();

        var provider = CreateServiceProvider(serviceCollection);
        var scope = provider.CreateScope();
        ((IDisposable)provider).Dispose();

        Assert.Throws<ObjectDisposedException>(() => scope.ServiceProvider.GetService<IFakeService>());
    }

    [Fact]
    public void ScopeDispose_PreventsServiceResolution()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IFakeService, FakeService>();

        var provider = CreateServiceProvider(serviceCollection);
        var scope = provider.CreateScope();
        scope.Dispose();

        Assert.Throws<ObjectDisposedException>(() => scope.ServiceProvider.GetService<IFakeService>());
        //Check that resolution from root works
        Assert.NotNull(provider.CreateScope());
    }

    [Fact]
    public void GetService_DisposeOnSameThread_Throws()
    {
        var services = new ServiceCollection();
        services.AddSingleton<DisposeServiceProviderInCtor>();
        var sp = CreateServiceProvider(services);
        Assert.Throws<ObjectDisposedException>(() =>
        {
            // ctor disposes ServiceProvider
            var service = sp.GetRequiredService<DisposeServiceProviderInCtor>();
        });
    }

    [Fact]
    public void GetAsyncService_DisposeAsyncOnSameThread_ThrowsAndDoesNotHangAndDisposeAsyncGetsCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        var asyncDisposableResource = new AsyncDisposable();
        services.AddSingleton<DisposeServiceProviderInCtorAsyncDisposable>(sp =>
            new DisposeServiceProviderInCtorAsyncDisposable(asyncDisposableResource, sp));

        var sp = CreateServiceProvider(services);
        var doesNotHang = Task.Run(() =>
        {
            SingleThreadedSynchronizationContext.Run(() =>
            {
                // Act
                Assert.Throws<ObjectDisposedException>(() =>
                {
                    // ctor disposes ServiceProvider
                    var service = sp.GetRequiredService<DisposeServiceProviderInCtorAsyncDisposable>();
                });
            });
        }).Wait(TimeSpan.FromSeconds(20));

        Assert.True(doesNotHang, "!doesNotHang");
        Assert.True(asyncDisposableResource.DisposeAsyncCalled, "!DisposeAsyncCalled");
    }

    [Fact]
    public void GetService_DisposeOnSameThread_ThrowsAndDoesNotHangAndDisposeGetsCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        var disposableResource = new Disposable();
        services.AddSingleton<DisposeServiceProviderInCtorDisposable>(sp =>
            new DisposeServiceProviderInCtorDisposable(disposableResource, sp));

        var sp = CreateServiceProvider(services);
        var doesNotHang = Task.Run(() =>
        {
            SingleThreadedSynchronizationContext.Run(() =>
            {
                // Act
                Assert.Throws<ObjectDisposedException>(() =>
                {
                    // ctor disposes ServiceProvider
                    var service = sp.GetRequiredService<DisposeServiceProviderInCtorDisposable>();
                });
            });
        }).Wait(TimeSpan.FromSeconds(10));

        Assert.True(doesNotHang);
        Assert.True(disposableResource.Disposed);
    }

    private class DisposeServiceProviderInCtor : IDisposable
    {
        public DisposeServiceProviderInCtor(IServiceProvider sp)
        {
            (sp as IDisposable).Dispose();
        }
        public void Dispose() { }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AddDisposablesAndAsyncDisposables_DisposeAsync_AllDisposed(bool includeDelayedAsyncDisposable)
    {
        var services = new ServiceCollection();
        services.AddSingleton<AsyncDisposable>();
        services.AddSingleton<Disposable>();
        if (includeDelayedAsyncDisposable)
        {
            //forces Dispose ValueTask to be asynchronous and not be immediately completed
            services.AddSingleton<DelayedAsyncDisposableService>();
        }
        var sp = new SyringeServiceProvider(services);

        var disposable = sp.GetRequiredService<Disposable>();
        var asyncDisposable = sp.GetRequiredService<AsyncDisposable>();
        DelayedAsyncDisposableService delayedAsyncDisposableService = null;
        if (includeDelayedAsyncDisposable)
        {
            delayedAsyncDisposableService = sp.GetRequiredService<DelayedAsyncDisposableService>();
        }

        await sp.DisposeAsync();

        Assert.True(disposable.Disposed);
        Assert.True(asyncDisposable.DisposeAsyncCalled);
        if (includeDelayedAsyncDisposable)
        {
            Assert.Equal(1, delayedAsyncDisposableService.DisposeCount);
        }
    }

    private class DisposeServiceProviderInCtorAsyncDisposable : IFakeService, IAsyncDisposable
    {
        private readonly AsyncDisposable _asyncDisposable;

        public DisposeServiceProviderInCtorAsyncDisposable(AsyncDisposable asyncDisposable, IServiceProvider sp)
        {
            _asyncDisposable = asyncDisposable;
            (sp as IAsyncDisposable).DisposeAsync();
        }
        public async ValueTask DisposeAsync()
        {
            await _asyncDisposable.DisposeAsync();
            await Task.Yield();
        }
    }

    private class DisposeServiceProviderInCtorDisposable : IFakeService, IDisposable
    {
        private readonly Disposable _disposable;

        public DisposeServiceProviderInCtorDisposable(Disposable disposable, IServiceProvider sp)
        {
            _disposable = disposable;
            (sp as IDisposable).Dispose();
        }
        public void Dispose()
        {
            _disposable.Dispose();
        }
    }

    [ThreadStatic]
    public static int ThreadId;

    private class OuterSingleton
    {
        public InnerSingleton InnerSingleton;
        public OuterSingleton(InnerSingleton innerSingleton)
        {
            InnerSingleton = innerSingleton;
        }
    }

    private class InnerSingleton
    {
        public InnerSingleton(ManualResetEvent mre1, ManualResetEvent mre2)
        {
            // Making sure ctor gets called only once
            Assert.True(!mre1.WaitOne(0) && !mre2.WaitOne(0));

            // Then use mre2 to signal execution reached this ctor call
            mre2.Set();

            // Wait until it's OK to leave ctor
            mre1.WaitOne();
        }
    }

    [Fact]
    public async Task GetRequiredService_ResolvingSameSingletonInTwoThreads_SameServiceReturned()
    {
        using (var mreForThread1 = new ManualResetEvent(false))
        using (var mreForThread2 = new ManualResetEvent(false))
        using (var mreForThread3 = new ManualResetEvent(false))
        {
            InnerSingleton innerSingleton = null;
            OuterSingleton outerSingleton = null;
            IServiceProvider sp = null;

            // Arrange
            var services = new ServiceCollection();

            services.AddSingleton<OuterSingleton>();
            services.AddSingleton(_ => new InnerSingleton(mreForThread1, mreForThread2));

            sp = CreateServiceProvider(services);

            var t1 = Task.Run(() =>
            {
                outerSingleton = sp.GetRequiredService<OuterSingleton>();
            });

            // Wait until mre2 gets set in InnerSingleton ctor
            mreForThread2.WaitOne();

            var t2 = Task.Run(() =>
            {
                mreForThread3.Set();

                // This waits on InnerSingleton singleton lock that is taken in thread 1
                innerSingleton = sp.GetRequiredService<InnerSingleton>();
            });

            mreForThread3.WaitOne();

            // Set a timeout before unblocking execution of both thread1 and thread2 via mre1:
            Assert.False(mreForThread1.WaitOne(10));

            // By this time thread 1 has already reached InnerSingleton ctor and is waiting for mre1.
            // within the GetRequiredService call, thread 2 should be waiting on a singleton lock for InnerSingleton
            // (rather than trying to instantiating InnerSingleton twice).
            mreForThread1.Set();

            // Act
            await t1;
            await t2;

            // Assert
            Assert.NotNull(outerSingleton);
            Assert.NotNull(innerSingleton);
            Assert.Same(outerSingleton.InnerSingleton, innerSingleton);
        }
    }

    [Fact]
    public async Task GetRequiredService_UsesSingletonAndLazyLocks_NoDeadlock()
    {
        using (var mreForThread1 = new ManualResetEvent(false))
        using (var mreForThread2 = new ManualResetEvent(false))
        {
            // Thread 1: Thing1 (transient) -> Thing0 (singleton)
            // Thread 2: Thing2 (singleton) -> Thing1 (transient) -> Thing0 (singleton)

            // 1. Thread 1 resolves the Thing1 which is a transient service
            // 2. In parallel, Thread 2 resolves Thing2 which is a singleton
            // 3. Thread 1 enters the factory callback for Thing1 and takes the lazy lock
            // 4. Thread 2 takes callsite for Thing2 as a singleton lock when it resolves Thing2
            // 5. Thread 2 enters the factory callback for Thing1 and waits on the lazy lock
            // 6. Thread 1 calls GetRequiredService<Thing0> on the service provider, takes callsite for Thing0 causing no deadlock
            // (rather than taking the locks that are already taken - either the lazy lock or the Thing2 callsite lock)

            Thing0 thing0 = null;
            Thing1 thing1 = null;
            Thing2 thing2 = null;
            IServiceProvider sp = null;
            var sb = new StringBuilder();

            // Arrange
            var services = new ServiceCollection();

            var lazy = new Lazy<Thing1>(() =>
            {
                sb.Append("3");
                mreForThread2.Set();   // Now that thread 1 holds lazy lock, allow thread 2 to continue

                // by this time, Thread 2 is holding a singleton lock for Thing2,
                // and Thread one holds the lazy lock
                // the call below to resolve Thing0 does not hang
                // since singletons do not share the same lock upon resolve anymore.
                thing0 = sp.GetRequiredService<Thing0>();
                return new Thing1(thing0);
            });

            services.AddSingleton<Thing0>();
            services.AddTransient(_ =>
            {
                if (ThreadId == 2)
                {
                    sb.Append("1");
                    mreForThread1.Set();   // [b] Allow thread 1 to continue execution and take the lazy lock
                    mreForThread2.WaitOne();   // [c] Wait until thread 1 takes the lazy lock

                    sb.Append("4");
                }

                // Let Thread 1 over take Thread 2
                var value = lazy.Value;
                return value;
            });
            services.AddSingleton<Thing2>();

            sp = CreateServiceProvider(services);

            var t1 = Task.Run(() =>
            {
                ThreadId = 1;
                using var scope1 = sp.CreateScope();
                mreForThread1.WaitOne(); // [a] Waits until thread 2 reaches the transient call to ensure it holds Thing2 singleton lock

                sb.Append("2");
                thing1 = scope1.ServiceProvider.GetRequiredService<Thing1>();
            });

            var t2 = Task.Run(() =>
            {
                ThreadId = 2;
                using var scope2 = sp.CreateScope();
                thing2 = scope2.ServiceProvider.GetRequiredService<Thing2>();
            });

            // Act
            await t1;
            await t2;

            // Assert
            Assert.NotNull(thing0);
            Assert.NotNull(thing1);
            Assert.NotNull(thing2);
            Assert.Equal("1234", sb.ToString()); // Expected order of execution
        }
    }

    [Fact]
    public async Task GetRequiredService_BiggerObjectGraphWithOpenGenerics_NoDeadlock()
    {
        // Test is similar to GetRequiredService_UsesSingletonAndLazyLocks_NoDeadlock (but for open generics and a larger object graph)
        using (var mreForThread1 = new ManualResetEvent(false))
        using (var mreForThread2 = new ManualResetEvent(false))
        {
            // Arrange
            List<IFakeOpenGenericService<Thing4>> constrainedThing4Services = null;
            List<IFakeOpenGenericService<Thing5>> constrainedThing5Services = null;

            Thing3 thing3 = null;
            IServiceProvider sp = null;
            var sb = new StringBuilder();

            var services = new ServiceCollection();

            services.AddSingleton<Thing0>();
            services.AddSingleton<Thing1>();
            services.AddSingleton<Thing2>();
            services.AddSingleton<Thing3>();
            services.AddTransient(typeof(IFakeOpenGenericService<>), typeof(FakeOpenGenericService<>));

            var lazy = new Lazy<Thing4>(() =>
            {
                sb.Append("3");
                mreForThread2.Set();   // Now that thread 1 holds lazy lock, allow thread 2 to continue

                thing3 = sp.GetRequiredService<Thing3>();
                return new Thing4(thing3);
            });

            services.AddTransient(_ =>
            {
                if (ThreadId == 2)
                {
                    sb.Append("1");
                    mreForThread1.Set();   // [b] Allow thread 1 to continue execution and take the lazy lock
                    mreForThread2.WaitOne();   // [c] Wait until thread 1 takes the lazy lock

                    sb.Append("4");
                }

                // Let Thread 1 over take Thread 2
                var value = lazy.Value;
                return value;
            });
            services.AddSingleton<Thing5>();

            sp = CreateServiceProvider(services);

            // Act
            var t1 = Task.Run(() =>
            {
                ThreadId = 1;
                using var scope1 = sp.CreateScope();
                mreForThread1.WaitOne(); // Waits until thread 2 reaches the transient call to ensure it holds Thing4 singleton lock

                sb.Append("2");
                constrainedThing4Services = sp.GetServices<IFakeOpenGenericService<Thing4>>().ToList();
            });

            var t2 = Task.Run(() =>
            {
                ThreadId = 2;
                using var scope2 = sp.CreateScope();
                constrainedThing5Services = sp.GetServices<IFakeOpenGenericService<Thing5>>().ToList();
            });

            // Act
            await t1;
            await t2;

            Assert.Equal("1234", sb.ToString()); // Expected order of execution

            var thing4 = sp.GetRequiredService<Thing4>();
            var thing5 = sp.GetRequiredService<Thing5>();

            // Assert
            Assert.NotNull(thing3);
            Assert.NotNull(thing4);
            Assert.NotNull(thing5);
            Assert.Equal(1, constrainedThing4Services.Count);
            Assert.Equal(1, constrainedThing5Services.Count);
            Assert.Same(thing4, constrainedThing4Services[0].Value);
            Assert.Same(thing5, constrainedThing5Services[0].Value);
        }
    }

    private class Thing5
    {
        public Thing5(Thing4 thing)
        {
        }
    }

    private class Thing4
    {
        public Thing4(Thing3 thing)
        {
        }
    }

    private class Thing3
    {
        public Thing3(Thing2 thing)
        {
        }
    }

    private class Thing2
    {
        public Thing2(Thing1 thing1)
        {
        }
    }

    private class Thing1
    {
        public Thing1(Thing0 thing0)
        {
        }
    }

    private class Thing0 { }

    [Theory]
    [InlineData(ServiceLifetime.Transient)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Singleton)]
    public void WorksWithStructServices(ServiceLifetime lifetime)
    {
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.Add(new ServiceDescriptor(typeof(IFakeService), typeof(StructFakeService), lifetime));
        serviceCollection.Add(new ServiceDescriptor(typeof(StructService), typeof(StructService), lifetime));
        serviceCollection.Add(new ServiceDescriptor(typeof(IFakeMultipleService), typeof(StructFakeMultipleService), lifetime));

        var provider = CreateServiceProvider(serviceCollection);
        var service = provider.GetService<IFakeMultipleService>();

        Assert.NotNull(service);
        Assert.IsType<StructFakeMultipleService>(service);
    }

    [Theory]
    [InlineData(ServiceLifetime.Transient)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Singleton)]
    public void WorksWithFactoryStructServices(ServiceLifetime lifetime)
    {
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.Add(new ServiceDescriptor(typeof(IFakeService), _ => new StructServiceWithNoDependencies(), lifetime));

        var provider = CreateServiceProvider(serviceCollection);
        var service = provider.GetService<IFakeService>();

        Assert.NotNull(service);
        Assert.IsType<StructServiceWithNoDependencies>(service);
    }

    [Theory]
    [InlineData(ServiceLifetime.Transient)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Singleton)]
    public void WorksWithFactoryStructServicesAsDependencies(ServiceLifetime lifetime)
    {
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.Add(new ServiceDescriptor(typeof(IFakeService), _ => new StructServiceWithNoDependencies(), lifetime));
        serviceCollection.Add(new ServiceDescriptor(typeof(StructService), typeof(StructService), lifetime));
        serviceCollection.Add(new ServiceDescriptor(typeof(IFakeMultipleService), typeof(StructFakeMultipleService), lifetime));

        var provider = CreateServiceProvider(serviceCollection);
        var service = provider.GetService<IFakeMultipleService>();

        Assert.NotNull(service);
        Assert.IsType<StructFakeMultipleService>(service);
    }

    [Theory]
    [InlineData(ServiceLifetime.Transient)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Singleton)]
    public void WorksWithIEnumerableStructServices(ServiceLifetime lifetime)
    {
        IServiceCollection serviceCollection = new ServiceCollection();
        for (var i = 0; i < 10; i++)
        {
            serviceCollection.Add(new ServiceDescriptor(typeof(IFakeService), typeof(StructServiceWithNoDependencies), lifetime));
        }

        var provider = CreateServiceProvider(serviceCollection);
        var services = provider.GetService<IEnumerable<IFakeService>>();

        Assert.Equal(10, services.Count());
        Assert.All(services, service => Assert.IsType<StructServiceWithNoDependencies>(service));
    }

    [Fact]
    public void WorksWithWideScopedTrees()
    {
        var serviceCollection = new ServiceCollection();
        for (var i = 0; i < 20; i++)
        {
            serviceCollection.AddScoped<IFakeOuterService, FakeOuterService>();
            serviceCollection.AddScoped<IFakeMultipleService, FakeMultipleServiceWithIEnumerableDependency>();
            serviceCollection.AddScoped<IFakeService, FakeService>();
        }
        var serviceProvider = CreateServiceProvider(serviceCollection);

        var services = serviceProvider.GetService<IEnumerable<IFakeOuterService>>();

        Assert.Equal(20, services.Count());
    }

    [Fact]
    public void GenericIEnumerableItemCachedInTheRightSlot()
    {
        var services = new ServiceCollection();
        // It's important that this service is generic, it hits a different codepath when resolved inside IEnumerable
        services.AddSingleton<IFakeOpenGenericService<PocoClass>, FakeService>();
        // Doesn't matter what this services is, we just want something in the collection after generic registration
        services.AddSingleton<FakeService>();

        var serviceProvider = CreateServiceProvider(services);

        var serviceRef1 = serviceProvider.GetRequiredService<IFakeOpenGenericService<PocoClass>>();
        var servicesRef1 = serviceProvider.GetServices<IFakeOpenGenericService<PocoClass>>().Single();

        Assert.Same(serviceRef1, servicesRef1);
    }

    [Fact]
    public async Task ProviderDisposeAsyncCallsDisposeAsyncOnServices()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<AsyncDisposable>();

        var serviceProvider = CreateServiceProvider(serviceCollection);
        var disposable = serviceProvider.GetService<AsyncDisposable>();

        await (serviceProvider as IAsyncDisposable).DisposeAsync();

        Assert.True(disposable.DisposeAsyncCalled);
    }

    [Fact]
    public async Task ProviderDisposeAsyncPrefersDisposeAsyncOnServices()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<SyncAsyncDisposable>();

        var serviceProvider = CreateServiceProvider(serviceCollection);
        var disposable = serviceProvider.GetService<SyncAsyncDisposable>();

        await (serviceProvider as IAsyncDisposable).DisposeAsync();

        Assert.True(disposable.DisposeAsyncCalled);
    }

    [Fact]
    public void ProviderDisposePrefersServiceDispose()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<SyncAsyncDisposable>();

        var serviceProvider = CreateServiceProvider(serviceCollection);
        var disposable = serviceProvider.GetService<SyncAsyncDisposable>();

        (serviceProvider as IDisposable).Dispose();

        Assert.True(disposable.DisposeCalled);
    }

    [Fact]
    public void ProviderDisposeThrowsWhenOnlyDisposeAsyncImplemented()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<AsyncDisposable>();

        var serviceProvider = CreateServiceProvider(serviceCollection);
        var disposable = serviceProvider.GetService<AsyncDisposable>();

        var exception = Assert.Throws<InvalidOperationException>(() => (serviceProvider as IDisposable).Dispose());
        Assert.Equal(
            "'MicrosoftCopy.DependencyInjection.Tests.ServiceProviderContainerTests+AsyncDisposable' type only implements IAsyncDisposable. Use DisposeAsync to dispose the container.",
            exception.Message);
    }

    [Fact]
    public async Task ProviderScopeDisposeAsyncCallsDisposeAsyncOnServices()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<AsyncDisposable>();

        var serviceProvider = CreateServiceProvider(serviceCollection);
        var scope = serviceProvider.CreateScope();
        var disposable = scope.ServiceProvider.GetService<AsyncDisposable>();

        await (scope as IAsyncDisposable).DisposeAsync();

        Assert.True(disposable.DisposeAsyncCalled);
    }

    [Fact]
    public async Task ProviderScopeDisposeAsyncPrefersDisposeAsyncOnServices()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<SyncAsyncDisposable>();

        var serviceProvider = CreateServiceProvider(serviceCollection);
        var scope = serviceProvider.CreateScope();
        var disposable = scope.ServiceProvider.GetService<SyncAsyncDisposable>();

        await (scope as IAsyncDisposable).DisposeAsync();

        Assert.True(disposable.DisposeAsyncCalled);
    }

    [Fact]
    public void ProviderScopeDisposePrefersServiceDispose()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<SyncAsyncDisposable>();

        var serviceProvider = CreateServiceProvider(serviceCollection);
        var scope = serviceProvider.CreateScope();
        var disposable = scope.ServiceProvider.GetService<SyncAsyncDisposable>();

        scope.Dispose();

        Assert.True(disposable.DisposeCalled);
    }

    [Fact]
    public void ProviderScopeDisposeThrowsWhenOnlyDisposeAsyncImplemented()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<AsyncDisposable>();

        var serviceProvider = CreateServiceProvider(serviceCollection);
        var scope = serviceProvider.CreateScope();
        var disposable = scope.ServiceProvider.GetService<AsyncDisposable>();

        var exception = Assert.Throws<InvalidOperationException>(() => scope.Dispose());
        Assert.Equal(
            "'MicrosoftCopy.DependencyInjection.Tests.ServiceProviderContainerTests+AsyncDisposable' type only implements IAsyncDisposable. Use DisposeAsync to dispose the container.",
            exception.Message);
    }

    [Fact]
    public async Task ProviderAsyncScopeDisposeAsyncCallsDisposeAsyncOnServices()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<AsyncDisposable>();

        var serviceProvider = CreateServiceProvider(serviceCollection);
        var scope = serviceProvider.CreateAsyncScope();
        var disposable = scope.ServiceProvider.GetService<AsyncDisposable>();

        await scope.DisposeAsync();

        Assert.True(disposable.DisposeAsyncCalled);
    }

    [Fact]
    public async Task ProviderAsyncScopeDisposeAsyncPrefersDisposeAsyncOnServices()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<SyncAsyncDisposable>();

        var serviceProvider = CreateServiceProvider(serviceCollection);
        var scope = serviceProvider.CreateAsyncScope();
        var disposable = scope.ServiceProvider.GetService<SyncAsyncDisposable>();

        await scope.DisposeAsync();

        Assert.True(disposable.DisposeAsyncCalled);
    }

    [Fact]
    public void ProviderAsyncScopeDisposePrefersServiceDispose()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<SyncAsyncDisposable>();

        var serviceProvider = CreateServiceProvider(serviceCollection);
        var scope = serviceProvider.CreateScope();
        var disposable = scope.ServiceProvider.GetService<SyncAsyncDisposable>();

        scope.Dispose();

        Assert.True(disposable.DisposeCalled);
    }

    [Fact]
    public void ProviderAsyncScopeDisposeThrowsWhenOnlyDisposeAsyncImplemented()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<AsyncDisposable>();

        var serviceProvider = CreateServiceProvider(serviceCollection);
        var scope = serviceProvider.CreateScope();
        var disposable = scope.ServiceProvider.GetService<AsyncDisposable>();

        var exception = Assert.Throws<InvalidOperationException>(() => scope.Dispose());
        Assert.Equal(
            "'MicrosoftCopy.DependencyInjection.Tests.ServiceProviderContainerTests+AsyncDisposable' type only implements IAsyncDisposable. Use DisposeAsync to dispose the container.",
            exception.Message);
    }

    [Fact]
    public void SingletonServiceCreatedFromFactoryIsDisposedWhenContainerIsDisposed()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(_ => new FakeDisposable());
        var serviceProvider = CreateServiceProvider(serviceCollection);

        // Act
        var service = serviceProvider.GetService<FakeDisposable>();
        ((IDisposable)serviceProvider).Dispose();

        // Assert
        Assert.True(service.IsDisposed);
    }

    [Fact]
    public void SingletonServiceCreatedFromInstanceIsNotDisposedWhenContainerIsDisposed()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(new FakeDisposable());
        var serviceProvider = CreateServiceProvider(serviceCollection);

        // Act
        var service = serviceProvider.GetService<FakeDisposable>();
        ((IDisposable)serviceProvider).Dispose();

        // Assert
        Assert.False(service.IsDisposed);
    }

    [Fact]
    public async Task ProviderDisposeAsyncCallsDisposeAsyncOnceOnServices()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<DelayedAsyncDisposableService>();

        var serviceProvider = CreateServiceProvider(serviceCollection);
        var disposable = serviceProvider.GetService<DelayedAsyncDisposableService>();

        await (serviceProvider as IAsyncDisposable).DisposeAsync();

        Assert.Equal(1, disposable.DisposeCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ValidateOnBuild_True_ResolvesConstrainedOpenGeneric(bool validateOnBuild)
    {
        var services = new ServiceCollection();

        services.AddTransient<IBB<AA>, BB>();
        services.AddTransient(typeof(IBB<>), typeof(GenericBB<>));
        services.AddTransient(typeof(IBB<>), typeof(ConstrainedGenericBB<>));

        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = validateOnBuild });
        var handlers = serviceProvider
            .GetServices<IBB<AA>>()
            .ToList();

        Assert.Equal(3, handlers.Count);
        var handlersTypes = handlers
            .Select(h => h.GetType())
            .ToList();

        Assert.Contains(typeof(BB), handlersTypes);
        Assert.Contains(typeof(GenericBB<AA>), handlersTypes);
        Assert.Contains(typeof(ConstrainedGenericBB<AA>), handlersTypes);
    }

    private class FakeDisposable : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    private class FakeMultipleServiceWithIEnumerableDependency : IFakeMultipleService
    {
        public FakeMultipleServiceWithIEnumerableDependency(IEnumerable<IFakeService> fakeServices)
        {
        }
    }

    private abstract class AbstractFakeOpenGenericService<T> : IFakeOpenGenericService<T>
    {
        public abstract T Value { get; }
    }

    private class FakeOpenGenericServiceWithTwoTypeArguments<TVal1, TVal2> : IFakeOpenGenericService<TVal1>
    {
        public TVal1 Value { get; }
        public TVal2 Value2 { get; }
    }

    private class Disposable : IDisposable
    {
        public bool Disposed { get; set; }

        public void Dispose()
        {
            Disposed = true;
        }
    }

    private class AsyncDisposable : IFakeService, IAsyncDisposable
    {
        public bool DisposeAsyncCalled { get; private set; }

        public ValueTask DisposeAsync()
        {
            DisposeAsyncCalled = true;
            return new ValueTask(Task.CompletedTask);
        }
    }

    private class SyncAsyncDisposable : IFakeService, IAsyncDisposable, IDisposable
    {
        public bool DisposeCalled { get; private set; }
        public bool DisposeAsyncCalled { get; private set; }

        public void Dispose()
        {
            DisposeCalled = true;
        }

        public ValueTask DisposeAsync()
        {
            DisposeAsyncCalled = true;
            return new ValueTask(Task.CompletedTask);
        }
    }

    private class DelayedAsyncDisposableService : IAsyncDisposable
    {
        public int DisposeCount { get; private set; }
        public async ValueTask DisposeAsync()
        {
            //forces ValueTask to be asynchronous and not be immediately completed
            await Task.Yield();
            DisposeCount++;
        }
    }

    [Fact]
    public async Task GetRequiredService_ResolveUniqueServicesConcurrently_StressTestSuccessful()
    {
        for (var i = 0; i < 100; i++)
        {
            Assert.True(await ResolveUniqueServicesConcurrently());
        }
    }

    [Fact]
    public void ScopedServiceResolvedFromSingletonAfterCompilation()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<A>();
        var sp = CreateServiceProvider(serviceCollection);

        var singleton = sp.GetRequiredService<A>();
        for (var i = 0; i < 10; i++)
        {
            Assert.Same(singleton, sp.GetRequiredService<A>());
            Thread.Sleep(10); // Give the background thread time to compile
        }
    }

    [Fact]
    public void ScopedServiceResolvedFromSingletonAfterCompilation2()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<A>()
            .AddSingleton<IFakeOpenGenericService<A>, FakeOpenGenericService<A>>();
        var sp = CreateServiceProvider(serviceCollection);

        var scope = sp.CreateScope();
        for (var i = 0; i < 50; i++)
        {
            scope.ServiceProvider.GetRequiredService<A>();
            Thread.Sleep(10); // Give the background thread time to compile
        }

        Assert.Same(sp.GetRequiredService<IFakeOpenGenericService<A>>().Value, sp.GetRequiredService<A>());
    }

    [Fact]
    public void ScopedServiceResolvedFromSingletonAfterCompilation3()
    {
        // Singleton IFakeX<A> -> Scoped A -> Scoped Aa
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<Aa>()
            .AddScoped<A>()
            .AddSingleton<IFakeOpenGenericService<Aa>, FakeOpenGenericService<Aa>>();
        var sp = CreateServiceProvider(serviceCollection);

        var scope = sp.CreateScope();
        for (var i = 0; i < 50; i++)
        {
            scope.ServiceProvider.GetRequiredService<A>();
            Thread.Sleep(10); // Give the background thread time to compile
        }

        Assert.Same(sp.GetRequiredService<IFakeOpenGenericService<Aa>>().Value.PropertyA, sp.GetRequiredService<A>());
    }

    [Fact]
    public void ResolveKeyedServiceWithKeyedParameter_MissingRegistrationButWithUnkeyedService()
    {
        var serviceCollection = new ServiceCollection();

        // We are not registering "service1" and "service1" keyed IService services and OtherService requires them,
        // but we are registering an unkeyed IService service which should not be injected into OtherService.
        serviceCollection.AddSingleton<KeyedDependencyInjectionSpecificationTests.IService, KeyedDependencyInjectionSpecificationTests.Service>();

        serviceCollection.AddSingleton<KeyedDependencyInjectionSpecificationTests.OtherService>();

        var ex = Assert.Throws<AggregateException>(() => serviceCollection.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true
        }));

        Assert.Equal(1, ex.InnerExceptions.Count);
        Assert.StartsWith("Some services are not able to be constructed", ex.Message);
        Assert.Contains("ServiceType: Microsoft.Extensions.DependencyInjection.Specification.KeyedDependencyInjectionSpecificationTests+OtherService", ex.ToString());
        Assert.Contains("Microsoft.Extensions.DependencyInjection.Specification.KeyedDependencyInjectionSpecificationTests+IService", ex.ToString());
    }

    private async Task<bool> ResolveUniqueServicesConcurrently()
    {
        var types = new[]
        {
            typeof(A), typeof(B), typeof(C), typeof(D), typeof(E),
            typeof(F), typeof(G), typeof(H), typeof(I), typeof(J)
        };

        IServiceProvider sp = null;
        var services = new ServiceCollection();
        foreach (var type in types)
        {
            services.AddSingleton(type);
        }

        sp = services.BuildServiceProvider();
        var tasks = new List<Task<bool>>();
        foreach (var type in types)
        {
            tasks.Add(Task.Run(() =>
                sp.GetRequiredService(type) != null)
            );
        }

        await Task<bool>.WhenAll(tasks);
        var succeeded = true;
        foreach (var task in tasks)
        {
            if (!task.Result)
            {
                succeeded = false;
                break;
            }
        }
        return succeeded;
    }

    private class A { }
    private class B { }
    private class C { }
    private class D { }
    private class E { }
    private class F { }
    private class G { }
    private class H { }
    private class I { }
    private class J { }
    private class Aa
    {
        public Aa(A a)
        {
            PropertyA = a;
        }
        public A PropertyA { get; }
    }
    private interface IAA { }
    private interface IBB<T> { }
    private class AA : IAA { }
    private class BB : IBB<AA> { }
    private class GenericBB<T> : IBB<T> { }
    private class ConstrainedGenericBB<T> : IBB<T> where T : IAA { }
}