using System.Collections.Concurrent;
using Fluxor;
using RonSijm.Syringe.DependencyInjection;
using TestTypes = RonSijm.Syringe.Fluxor.Tests.Features.WireEffects.DynamicMethodEffectsFixtures<int>;

namespace RonSijm.Syringe.Fluxor.Tests.Features.WireEffects;

public sealed class DynamicMethodEffectsTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InstanceMethodsReceivePropertyInjectionAndKeepAllHandlers(bool dynamic)
    {
        await using var provider = await CreateProviderAsync(dynamic, 2, typeof(TestTypes.InstanceMethods));
        var store = provider.GetRequiredService<IStore>();
        var recorder = provider.GetRequiredService<TestTypes.Recorder>();
        provider.GetRequiredService<IDispatcher>().Dispatch(new TestTypes.Request());
        await recorder.Completed.Task.WaitAsync(TimeSpan.FromSeconds(2));
        recorder.Names.Should().BeEquivalentTo("first", "second");
        provider.GetRequiredService<TestTypes.InstanceMethods>().State.Should().BeSameAs(provider.GetRequiredService<IState<TestTypes.State>>());
        store.Should().BeSameAs(provider.GetRequiredService<IStore>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StaticMethodsSupportPayloadAndDispatcherOnlySignatures(bool dynamic)
    {
        await using var provider = await CreateProviderAsync(dynamic, 2, typeof(TestTypes.StaticMethods));
        provider.GetRequiredService<IDispatcher>().Dispatch(new TestTypes.Request());
        provider.GetRequiredService<IDispatcher>().Dispatch(new DispatcherOnlyRequest());
        var recorder = provider.GetRequiredService<TestTypes.Recorder>();
        await recorder.Completed.Task.WaitAsync(TimeSpan.FromSeconds(2));
        recorder.Names.Should().BeEquivalentTo("static", "dispatcher-only");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConstructorInjectedAsyncMethodsSupportBothRegistrationPaths(bool dynamic)
    {
        await using var provider = await CreateProviderAsync(dynamic, 1, typeof(TestTypes.ConstructorMethods));
        provider.GetRequiredService<IDispatcher>().Dispatch(new TestTypes.Request());
        var recorder = provider.GetRequiredService<TestTypes.Recorder>();
        await recorder.Completed.Task.WaitAsync(TimeSpan.FromSeconds(2));
        recorder.Names.Should().Equal("constructor");
    }

    [Fact]
    public async Task MultipleHostsForTheSameActionAndRebuildDoNotLoseOrDuplicateMethods()
    {
        await using var provider = await CreateProviderAsync(true, 3, typeof(TestTypes.InstanceMethods), typeof(TestTypes.ConstructorMethods));
        var store = provider.GetRequiredService<IStore>();
        provider.Build();
        provider.Build();
        provider.GetRequiredService<IDispatcher>().Dispatch(new TestTypes.Request());
        var recorder = provider.GetRequiredService<TestTypes.Recorder>();
        await recorder.Completed.Task.WaitAsync(TimeSpan.FromSeconds(2));
        recorder.Names.Should().BeEquivalentTo("first", "second", "constructor");
        store.Should().BeSameAs(provider.GetRequiredService<IStore>());
    }

    [Fact]
    public async Task DynamicMethodsAndTypedEffectsHandleTheSameActionTogether()
    {
        await using var provider = await CreateProviderAsync(true, 3, typeof(TestTypes.InstanceMethods), typeof(TestTypes.TypedEffect));
        provider.GetRequiredService<IDispatcher>().Dispatch(new TestTypes.Request());
        var recorder = provider.GetRequiredService<TestTypes.Recorder>();
        await recorder.Completed.Task.WaitAsync(TimeSpan.FromSeconds(2));
        recorder.Names.Should().BeEquivalentTo("first", "second", "typed");
    }

    [Fact]
    public async Task DisablingPropertyInjectionLeavesMethodHostPropertiesUntouched()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new TestTypes.Recorder(1));
        await using var provider = new SyringeServiceProvider(services, options => options.UseFluxor(fluxor =>
        {
            fluxor.DisablePropertyInjection = true;
            fluxor.ScanTypes(typeof(TestTypes.State), typeof(TestTypes.Reducers));
        }));
        await provider.GetRequiredService<IStore>().InitializeAsync();
        await AddMethodsAsync(provider, typeof(TestTypes.ConstructorMethods));
        var host = provider.GetRequiredService<TestTypes.ConstructorMethods>();
        host.OptionalRecorder.Should().BeNull();
        provider.GetRequiredService<IDispatcher>().Dispatch(new TestTypes.Request());
        await provider.GetRequiredService<TestTypes.Recorder>().Completed.Task.WaitAsync(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void MissingRequiredMethodHostDependencyFailsDuringStartupWiring()
    {
        var error = Assert.Throws<InvalidOperationException>(() => new SyringeServiceProvider(options => options.UseFluxor(fluxor => fluxor.ScanTypes(typeof(TestTypes.State), typeof(TestTypes.MissingDependencyMethods)))));
        error.Message.Should().Contain(nameof(TestTypes.MissingDependency));
    }

    [Fact]
    public async Task MultipleMethodResultsDrainTheQueueAndKeepTheLatestProjection()
    {
        await using var provider = await CreateProviderAsync(true, 2, typeof(TestTypes.SnapshotMethods), typeof(TestTypes.Aggregate));
        await Task.Run(() => provider.GetRequiredService<IDispatcher>().Dispatch(new TestTypes.Request())).WaitAsync(TimeSpan.FromSeconds(2));
        var state = provider.GetRequiredService<IState<TestTypes.State>>();
        var aggregate = provider.GetRequiredService<IState<TestTypes.Aggregate>>();
        state.Value.Handled.Should().Equal("first", "second");
        aggregate.Value.Child.Should().BeSameAs(state.Value);
    }

    [Fact]
    public async Task ExplicitStateRestorationStillUpdatesTheFeatureAndItsProjection()
    {
        await using var provider = await CreateProviderAsync(true, 1, typeof(TestTypes.Aggregate));
        var restored = new TestTypes.State { Handled = ["restored"] };
        await Task.Run(() => provider.GetRequiredService<IDispatcher>().Dispatch(restored)).WaitAsync(TimeSpan.FromSeconds(2));
        provider.GetRequiredService<IState<TestTypes.State>>().Value.Should().BeSameAs(restored);
        provider.GetRequiredService<IState<TestTypes.Aggregate>>().Value.Child.Should().BeSameAs(restored);
    }

    [Fact]
    public async Task MissingStateTypesCanBeRestoredAfterFeatureRegistration()
    {
        await using var provider = await CreateProviderAsync(true, 0, typeof(TestTypes.ConstructorMethods));
        var dispatcher = provider.GetRequiredService<IDispatcher>();
        dispatcher.Dispatch(new TestTypes.LateState { Count = 1 });
        await AddMethodsAsync(provider, typeof(TestTypes.LateState));
        dispatcher.Dispatch(new TestTypes.LateState { Count = 2 });
        provider.GetRequiredService<IState<TestTypes.LateState>>().Value.Count.Should().Be(2);
    }

    [Fact]
    public async Task LoadingAReducerAloneAttachesItToAnExistingFeature()
    {
        await using var provider = await CreateProviderAsync(true, 0, typeof(TestTypes.LateReducer));
        provider.GetRequiredService<IDispatcher>().Dispatch(new TestTypes.LateIncrement());
        provider.GetRequiredService<IState<TestTypes.State>>().Value.Handled.Should().Equal("late");
    }

    private static async Task<SyringeServiceProvider> CreateProviderAsync(bool dynamic, int expected, params Type[] effects)
    {
        var services = new ServiceCollection();
        var recorder = new TestTypes.Recorder(expected);
        services.AddSingleton(recorder);
        var provider = new SyringeServiceProvider(services, options => options.UseFluxor(fluxor =>
        {
            fluxor.ScanTypes(typeof(TestTypes.State), typeof(TestTypes.Reducers));
            if (!dynamic)
            {
                fluxor.ScanTypes(effects[0], effects.Skip(1).ToArray());
            }
        }));
        var store = provider.GetRequiredService<IStore>();
        store.UnhandledException += (_, args) => recorder.Completed.TrySetException(args.Exception);
        await store.InitializeAsync();
        var state = provider.GetRequiredService<IState<TestTypes.State>>();
        var handledCount = 0;
        state.StateChanged += (_, _) =>
        {
            foreach (var name in state.Value.Handled.Skip(handledCount))
            {
                recorder.Record(name);
            }
            handledCount = state.Value.Handled.Count;
        };
        if (dynamic)
        {
            await AddMethodsAsync(provider, effects);
        }
        return provider;
    }

    private static async Task AddMethodsAsync(SyringeServiceProvider provider, params Type[] effects)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddFluxorLibrary(options => options.ScanTypes(effects[0], effects.Skip(1).ToArray()));
        await provider.LoadServiceDescriptors(services);
        provider.Build();
    }
}

public sealed class InjectAttribute : Attribute
{
    public bool Required { get; set; }
}

public sealed record DispatcherOnlyRequest;

// Open generic fixtures stay out of the existing tests' whole-assembly scans.
public static class DynamicMethodEffectsFixtures<T>
{
    [FeatureState]
    public sealed record State
    {
        public IReadOnlyList<string> Handled { get; init; } = [];
    }

    public sealed record Request;
    public sealed record LateIncrement;
    public sealed record Handled(string Name);

    [FeatureState]
    public sealed record Aggregate
    {
        [ReduceInto]
        public State? Child { get; init; }
    }

    [FeatureState]
    public sealed record LateState
    {
        public int Count { get; init; }
    }

    public sealed class LateReducer : Reducer<State, LateIncrement>
    {
        public override State Reduce(State state, LateIncrement action)
        {
            return state with { Handled = [.. state.Handled, "late"] };
        }
    }

    public static class Reducers
    {
        [ReducerMethod]
        public static State Record(State state, Handled action)
        {
            return state with { Handled = [.. state.Handled, action.Name] };
        }
    }

    public sealed class Recorder(int expected)
    {
        private ConcurrentQueue<string> Calls { get; } = new();
        public TaskCompletionSource Completed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public string[] Names => Calls.ToArray();

        public void Record(string name)
        {
            Calls.Enqueue(name);
            if (Calls.Count >= expected)
            {
                Completed.TrySetResult();
            }
        }
    }

    public sealed class InstanceMethods
    {
        [Inject(Required = true)] public Recorder Recorder { get; set; } = null!;
        [Inject(Required = true)] public IState<State> State { get; set; } = null!;

        [EffectMethod]
        public Task First(Request action, IDispatcher dispatcher)
        {
            Recorder.Record("first");
            return Task.CompletedTask;
        }

        [EffectMethod]
        public Task Second(Request action, IDispatcher dispatcher)
        {
            Recorder.Record("second");
            return Task.CompletedTask;
        }
    }

    public sealed class ConstructorMethods(Recorder recorder)
    {
        [Inject] public Recorder? OptionalRecorder { get; set; }

        [EffectMethod]
        public async Task Handle(Request action, IDispatcher dispatcher)
        {
            await Task.Yield();
            recorder.Record("constructor");
        }
    }

    public static class StaticMethods
    {
        [EffectMethod]
        public static Task Handle(Request action, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new Handled("static"));
            return Task.CompletedTask;
        }

        [EffectMethod(typeof(DispatcherOnlyRequest))]
        public static Task WithoutPayload(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new Handled("dispatcher-only"));
            return Task.CompletedTask;
        }
    }

    public static class SnapshotMethods
    {
        [EffectMethod]
        public static Task First(Request action, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new Handled("first"));
            return Task.CompletedTask;
        }

        [EffectMethod]
        public static Task Second(Request action, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new Handled("second"));
            return Task.CompletedTask;
        }
    }

    public sealed class TypedEffect(Recorder recorder) : Effect<Request>
    {
        public override Task HandleAsync(Request action, IDispatcher dispatcher)
        {
            recorder.Record("typed");
            return Task.CompletedTask;
        }
    }

    public sealed class MissingDependency;

    public sealed class MissingDependencyMethods
    {
        [Inject(Required = true)] public MissingDependency Dependency { get; set; } = null!;

        [EffectMethod]
        public Task Handle(Request action, IDispatcher dispatcher)
        {
            return Task.CompletedTask;
        }
    }
}
