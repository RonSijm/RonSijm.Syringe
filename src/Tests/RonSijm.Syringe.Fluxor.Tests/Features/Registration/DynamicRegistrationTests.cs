using Fluxor;
using System.Reflection;
using System.Reflection.Emit;
using RonSijm.Syringe.DependencyInjection;
using RonSijm.Syringe.Fluxor.Tests.Features.WireEffects;
using Types = RonSijm.Syringe.Fluxor.Tests.Features.Registration.RegistrationFixtures<int>;

namespace RonSijm.Syringe.Fluxor.Tests.Features.Registration;

public class DynamicRegistrationTests
{
    [Theory]
    [InlineData(StoreLifetime.Singleton)]
    [InlineData(StoreLifetime.Scoped)]
    public async Task LifetimesAreHonoredAndExistingAndFutureScopesReceiveModules(StoreLifetime lifetime)
    {
        await using var provider = CreateProvider(lifetime);
        var first = provider.CreateScope().ServiceProvider;
        var second = provider.CreateScope().ServiceProvider;
        var firstStore = first.GetRequiredService<IStore>();
        var secondStore = second.GetRequiredService<IStore>();
        await firstStore.InitializeAsync();
        await secondStore.InitializeAsync();
        if (lifetime == StoreLifetime.Scoped)
        {
            firstStore.Should().NotBeSameAs(secondStore);
            first.GetRequiredService<FeatureCache>().Should().NotBeSameAs(second.GetRequiredService<FeatureCache>());
        }
        else
        {
            firstStore.Should().BeSameAs(secondStore);
        }

        await AddModule(provider, typeof(Types.Methods), typeof(Types.TypedLateReducer), typeof(Types.LateState));
        first.GetRequiredService<IDispatcher>().Dispatch(new Types.Increment());
        first.GetRequiredService<IState<Types.State>>().Value.Count.Should().Be(1);
        first.GetRequiredService<IStore>().Should().BeSameAs(firstStore);
        if (lifetime == StoreLifetime.Scoped)
        {
            second.GetRequiredService<IState<Types.State>>().Value.Count.Should().Be(0);
        }
        first.GetRequiredService<Types.Methods>().State.Should().BeSameAs(first.GetRequiredService<IState<Types.State>>());
        second.GetRequiredService<Types.Methods>().State.Should().BeSameAs(second.GetRequiredService<IState<Types.State>>());

        var third = provider.CreateScope().ServiceProvider;
        await third.GetRequiredService<IStore>().InitializeAsync();
        third.GetRequiredService<IDispatcher>().Dispatch(new Types.Increment());
        var expected = lifetime == StoreLifetime.Singleton ? 2 : 1;
        third.GetRequiredService<IState<Types.State>>().Value.Count.Should().Be(expected);
        await first.DisposeAsync();
        second.GetRequiredService<IStore>().Should().BeSameAs(secondStore);
    }

    [Fact]
    public async Task SharedFeaturesReducersAndEffectsAreInstalledOnceAcrossModules()
    {
        await using var provider = CreateProvider(StoreLifetime.Singleton);
        var store = provider.GetRequiredService<IStore>();
        await store.InitializeAsync();
        await AddModule(provider, typeof(Types.State), typeof(Types.Methods), typeof(Types.TypedLateReducer), typeof(Types.MethodReducers));
        await AddModule(provider, typeof(Types.State), typeof(Types.Methods), typeof(Types.TypedLateReducer), typeof(Types.MethodReducers), typeof(Types.LateState));
        provider.Build();
        provider.GetRequiredService<IDispatcher>().Dispatch(new Types.Increment());
        provider.GetRequiredService<IDispatcher>().Dispatch(new Types.MethodIncrement());
        provider.GetRequiredService<IState<Types.State>>().Value.Count.Should().Be(11);
        provider.GetRequiredService<Types.Recorder>().Calls.Should().Be(1);
        store.Features.Values.Count(feature => feature.GetStateType() == typeof(Types.State)).Should().Be(1);
        store.Should().BeSameAs(provider.GetRequiredService<IStore>());
    }

    [Fact]
    public async Task TwoStandaloneModulesCanShareRegistrationsDuringStartup()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Types.Recorder>();
        services.AddFluxorLibrary(options => options.ScanTypes(typeof(Types.State), typeof(Types.Methods), typeof(Types.TypedLateReducer)));
        services.AddFluxorLibrary(options => options.ScanTypes(typeof(Types.State), typeof(Types.Methods), typeof(Types.TypedLateReducer), typeof(Types.LateState)));
        await using var provider = new SyringeServiceProvider(services, options => options.UseFluxor(fluxor => fluxor.DisableAddingFluxorItself = true));
        await provider.GetRequiredService<IStore>().InitializeAsync();
        provider.GetRequiredService<IDispatcher>().Dispatch(new Types.Increment());
        provider.GetRequiredService<IState<Types.State>>().Value.Count.Should().Be(1);
        provider.GetRequiredService<Types.Recorder>().Calls.Should().Be(1);
    }

    [Fact]
    public async Task LateInstanceAndStaticMethodReducersAreInjectedAndCoalesced()
    {
        await using var provider = CreateProvider(StoreLifetime.Singleton);
        await provider.GetRequiredService<IStore>().InitializeAsync();
        await AddModule(provider, typeof(Types.MethodReducers), typeof(Types.InstanceReducer));
        await AddModule(provider, typeof(Types.MethodReducers), typeof(Types.InstanceReducer));
        provider.GetRequiredService<IDispatcher>().Dispatch(new Types.MethodIncrement());
        provider.GetRequiredService<IState<Types.State>>().Value.Count.Should().Be(110);
        provider.GetRequiredService<Types.InstanceReducer>().Recorder.Should().BeSameAs(provider.GetRequiredService<Types.Recorder>());
    }

    [Fact]
    public async Task FailedPreparationDoesNotAddAnyServicesOrFeaturesAndAllowsCleanRetry()
    {
        await using var provider = CreateProvider(StoreLifetime.Singleton);
        var store = provider.GetRequiredService<IStore>();
        await store.InitializeAsync();
        var features = store.Features.ToArray();
        var services = provider.ServiceDescriptors.ToArray();
        var module = new ServiceCollection();
        module.AddSingleton<Types.DisposableDependency>();
        module.AddFluxorLibrary(options => options.ScanTypes(typeof(Types.LateState), typeof(Types.BrokenEffect)));
        await provider.LoadServiceDescriptors(module);
        var error = Assert.Throws<InvalidOperationException>(provider.Build);
        error.Message.Should().Contain(nameof(Types.MissingDependency));
        store.Features.Should().BeEquivalentTo(features);
        provider.ServiceDescriptors.Should().Equal(services);
        provider.GetService<Types.DisposableDependency>().Should().BeNull();
        provider.GetService<IFeature<Types.LateState>>().Should().BeNull();
        await AddModule(provider, typeof(Types.LateState), typeof(Types.TypedLateReducer));
        provider.GetRequiredService<IDispatcher>().Dispatch(new Types.Increment());
        provider.GetRequiredService<IState<Types.State>>().Value.Count.Should().Be(1);
    }

    [Fact]
    public async Task ConflictingFeatureNamesAreRejectedBeforeInstallingOtherModuleItems()
    {
        await using var provider = CreateProvider(StoreLifetime.Singleton);
        var store = provider.GetRequiredService<IStore>();
        await store.InitializeAsync();
        var features = store.Features.ToArray();
        var module = new ServiceCollection();
        module.AddFluxorLibrary(options => options.ScanTypes(typeof(Types.LateState), typeof(Types.NameConflict)));
        await provider.LoadServiceDescriptors(module);
        var error = Assert.Throws<InvalidOperationException>(provider.Build);
        error.Message.Should().Contain("already registered");
        store.Features.Should().BeEquivalentTo(features);
        provider.GetService<IFeature<Types.LateState>>().Should().BeNull();
    }

    [Fact]
    public async Task ConflictingDefinitionsForOneStateAreRejected()
    {
        await using var provider = CreateProvider(StoreLifetime.Singleton);
        var module = new ServiceCollection();
        module.AddFluxorLibrary(options => options.ScanTypes(typeof(Types.AlternativeFeature)));
        await provider.LoadServiceDescriptors(module);
        var error = Assert.Throws<InvalidOperationException>(provider.Build);
        error.Message.Should().Contain("Conflicting Fluxor feature definitions");
        provider.GetService<Types.AlternativeFeature>().Should().BeNull();
    }

    [Fact]
    public async Task WarmedEnumerablesAreUpdatedWithoutRepeatingExistingDescriptors()
    {
        await using var provider = CreateProvider(StoreLifetime.Singleton);
        var before = provider.GetServices<FeatureCache>().ToArray();
        var module = new ServiceCollection();
        module.AddSingleton<FeatureCache>(_ => new FeatureCache());
        await provider.LoadServiceDescriptors(module);
        provider.Build();
        provider.GetServices<FeatureCache>().Should().HaveCount(before.Length + 1);
    }

    [Fact]
    public async Task NativeExtensionsUsePublicRegistrationAndTypedScanAndLifetimeConfiguration()
    {
        await using var provider = new SyringeServiceProvider(options =>
        {
            options.ValidateScopes = true;
            options.UseFluxor(fluxor => fluxor.AddNativeExtension(native =>
            {
                native.ScanTypes(typeof(Types.State), typeof(Types.TypedLateReducer));
                native.WithLifetime(StoreLifetime.Scoped);
                Types.AddNativeMiddleware(native);
            }));
        });
        var first = provider.CreateScope().ServiceProvider;
        var second = provider.CreateScope().ServiceProvider;
        var store = first.GetRequiredService<IStore>();
        await store.InitializeAsync();
        first.GetRequiredService<IDispatcher>().Dispatch(new Types.Increment());
        first.GetRequiredService<IState<Types.State>>().Value.Count.Should().Be(1);
        second.GetRequiredService<IState<Types.State>>().Value.Count.Should().Be(0);
        first.GetRequiredService<Types.NativeMiddleware>().Count.Should().BeGreaterThan(0);
        store.GetMiddlewares().OfType<Types.NativeMiddleware>().Should().ContainSingle();
    }

    private static SyringeServiceProvider CreateProvider(StoreLifetime lifetime)
    {
        var services = new ServiceCollection();
        services.AddSingleton<Types.Recorder>();
        return new SyringeServiceProvider(services, options =>
        {
            options.ValidateScopes = true;
            options.UseFluxor(fluxor =>
            {
                fluxor.WithLifetime(lifetime);
                fluxor.ScanTypes(typeof(Types.State));
            });
        });
    }

    [Fact]
    public async Task SharedOrdinaryAndKeyedServicesAreCoalescedWithinFluxorModules()
    {
        await using var provider = CreateProvider(StoreLifetime.Singleton);
        for (var index = 0; index < 2; index++)
        {
            var module = new ServiceCollection();
            module.AddSingleton<Types.DisposableDependency>();
            module.AddKeyedSingleton<Types.DisposableDependency>("shared");
            module.AddFluxorLibrary(options => options.ScanTypes(typeof(Types.State)));
            await provider.LoadServiceDescriptors(module);
        }
        provider.Build();
        provider.GetServices<Types.DisposableDependency>().Should().ContainSingle();
        provider.GetKeyedServices<Types.DisposableDependency>("shared").Should().ContainSingle();
    }

    [Fact]
    public async Task ExplicitConflictingModuleLifetimesAreRejected()
    {
        await using var provider = CreateProvider(StoreLifetime.Scoped);
        var module = new ServiceCollection();
        module.AddFluxorLibrary(options =>
        {
            options.WithLifetime(StoreLifetime.Singleton);
            options.ScanTypes(typeof(Types.LateState));
        });
        await provider.LoadServiceDescriptors(module);
        Assert.Throws<InvalidOperationException>(provider.Build).Message.Should().Contain("lifetime");
        provider.CreateScope().ServiceProvider.GetService<IFeature<Types.LateState>>().Should().BeNull();
    }

    [Fact]
    public async Task NativeAssemblyScanningDiscoversTheConfiguredAssembly()
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName($"ScanFixture{Guid.NewGuid():N}"), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("main");
        var builder = module.DefineType("IsolatedState", TypeAttributes.Public | TypeAttributes.Sealed);
        builder.DefineDefaultConstructor(MethodAttributes.Public);
        builder.SetCustomAttribute(new CustomAttributeBuilder(typeof(FeatureStateAttribute).GetConstructor(Type.EmptyTypes)!, []));
        var stateType = builder.CreateType()!;
        await using var provider = new SyringeServiceProvider(options => options.UseFluxor(fluxor => fluxor.AddNativeExtension(native => native.ScanAssemblies(assembly))));
        var feature = (IFeature)provider.GetRequiredService(typeof(IFeature<>).MakeGenericType(stateType));
        feature.GetStateType().Should().Be(stateType);
        provider.GetRequiredService<IStore>().Features.Values.Should().Contain(feature);
    }

    [Fact]
    public async Task ExternallyRegisteredNativeStoresCanStillUseSyringeHooks()
    {
        var services = new ServiceCollection();
        global::Fluxor.ServiceCollectionExtensions.AddFluxor(services, options => options.WithLifetime(StoreLifetime.Singleton).ScanTypes(typeof(Types.State)));
        await using var provider = new SyringeServiceProvider(services, options => options.UseFluxor(fluxor => fluxor.DisableAddingFluxorItself = true));
        var store = provider.GetRequiredService<IStore>();
        await store.InitializeAsync();
        await AddModule(provider, typeof(Types.TypedLateReducer));
        provider.GetRequiredService<IDispatcher>().Dispatch(new Types.Increment());
        provider.GetRequiredService<IState<Types.State>>().Value.Count.Should().Be(1);
    }

    [Fact]
    public async Task FailedDescriptorStreamsDoNotStagePartialBatches()
    {
        await using var provider = CreateProvider(StoreLifetime.Singleton);
        await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadServiceDescriptors(FailedStream()));
        provider.Build();
        provider.GetService<Types.DisposableDependency>().Should().BeNull();
    }

    [Fact]
    public async Task CommitCallbackFailuresAreExplicitAndPreventUnsafeProviderReuse()
    {
        await using var provider = new SyringeServiceProvider(options => options.UseFluxor(fluxor => fluxor.ScanTypes(typeof(Types.ThrowingFeature))));
        await provider.GetRequiredService<IStore>().InitializeAsync();
        var module = new ServiceCollection();
        module.AddFluxorLibrary(options => options.ScanTypes(typeof(Types.TypedLateReducer)));
        await provider.LoadServiceDescriptors(module);
        Assert.Throws<TargetInvocationException>(provider.Build).InnerException!.Message.Should().Be("Commit callback failed.");
        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<IStore>()).Message.Should().Contain("cannot be reused");
        Assert.Throws<InvalidOperationException>(provider.Build).Message.Should().Contain("cannot be reused");
    }

    private static async IAsyncEnumerable<ServiceDescriptor> FailedStream()
    {
        yield return ServiceDescriptor.Singleton(typeof(Types.DisposableDependency), typeof(Types.DisposableDependency));
        await Task.Yield();
        throw new InvalidOperationException("Incomplete module.");
    }

    [Fact]
    public async Task RollbackDisposesDependenciesCreatedBeforeAConstructorFailure()
    {
        await using var provider = CreateProvider(StoreLifetime.Singleton);
        Types.DisposableDependency? dependency = null;
        var module = new ServiceCollection();
        module.AddSingleton(_ => dependency = new Types.DisposableDependency());
        module.AddFluxorLibrary(options => options.ScanTypes(typeof(Types.ThrowingConstructorEffect)));
        await provider.LoadServiceDescriptors(module);
        Assert.Throws<InvalidOperationException>(provider.Build).Message.Should().Be("Constructor failed.");
        dependency.Should().NotBeNull();
        dependency!.Disposed.Should().BeTrue();
        provider.GetService<Types.DisposableDependency>().Should().BeNull();
    }

    private static async Task AddModule(SyringeServiceProvider provider, params Type[] types)
    {
        var module = new ServiceCollection();
        module.AddFluxorLibrary(options => options.ScanTypes(types[0], types.Skip(1).ToArray()));
        await provider.LoadServiceDescriptors(module);
        provider.Build();
    }
}

public static class RegistrationFixtures<T>
{
    [FeatureState(Name = "shared-counter")]
    public sealed record State
    {
        public int Count { get; init; }
    }

    [FeatureState]
    public sealed record LateState;

    [FeatureState(Name = "shared-counter")]
    public sealed record NameConflict;

    public sealed record Increment;
    public sealed record MethodIncrement;
    public sealed class MissingDependency;
    public sealed class Recorder
    {
        public int Calls { get; set; }
    }

    public sealed class DisposableDependency : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Disposed = true;
        }
    }

    public sealed class Methods
    {
        [Inject(Required = true)] public IState<State> State { get; set; } = null!;
        [Inject(Required = true)] public Recorder Recorder { get; set; } = null!;

        [EffectMethod]
        public Task Handle(Increment action, IDispatcher dispatcher)
        {
            Recorder.Calls++;
            return Task.CompletedTask;
        }
    }

    public sealed class TypedLateReducer : Reducer<State, Increment>
    {
        public override State Reduce(State state, Increment action) => state with { Count = state.Count + 1 };
    }

    public static class MethodReducers
    {
        [ReducerMethod]
        public static State Reduce(State state, MethodIncrement action) => state with { Count = state.Count + 10 };
    }

    public sealed class InstanceReducer
    {
        [Inject(Required = true)] public Recorder Recorder { get; set; } = null!;

        [ReducerMethod]
        public State Reduce(State state, MethodIncrement action) => state with { Count = state.Count + 100 };
    }

    public sealed class BrokenEffect(DisposableDependency dependency) : Effect<Increment>
    {
        [Inject(Required = true)] public MissingDependency Missing { get; set; } = null!;
        public DisposableDependency Dependency { get; } = dependency;
        public override Task HandleAsync(Increment action, IDispatcher dispatcher) => Task.CompletedTask;
    }

    public sealed class ThrowingConstructorEffect : Effect<Increment>
    {
        public ThrowingConstructorEffect(DisposableDependency dependency)
        {
            throw new InvalidOperationException("Constructor failed.");
        }
        public override Task HandleAsync(Increment action, IDispatcher dispatcher) => Task.CompletedTask;
    }

    public sealed class AlternativeFeature : Feature<State>
    {
        public override string GetName() => "alternative";
        protected override State GetInitialState() => new();
    }

    public sealed class ThrowingFeature : Feature<State>
    {
        public override string GetName() => "throwing-feature";
        protected override State GetInitialState() => new();
        public override void AddReducer(IReducer<State> reducer)
        {
            throw new InvalidOperationException("Commit callback failed.");
        }
    }

    public sealed class NativeMiddleware : Middleware
    {
        public int Count { get; private set; }

        public override void AfterDispatch(object action)
        {
            Count++;
        }
    }

    public static void AddNativeMiddleware(global::Fluxor.DependencyInjection.FluxorOptions options)
    {
        options.AddMiddleware<NativeMiddleware>();
    }
}
