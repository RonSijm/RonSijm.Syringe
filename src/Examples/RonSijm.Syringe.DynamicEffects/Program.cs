using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe;
using RonSijm.Syringe.DependencyInjection;

namespace RonSijm.Syringe.DynamicEffects;

public static class Program
{
    public static async Task<int> Main()
    {
        var services = new ServiceCollection();
        services.AddSingleton<EffectDependency>();
        await using var provider = new SyringeServiceProvider(services, options => options.UseFluxor(fluxor => fluxor.ScanTypes(typeof(SampleState), typeof(SampleReducers), typeof(StartupMethods))));
        var store = provider.GetRequiredService<IStore>();
        await store.InitializeAsync();
        var state = provider.GetRequiredService<IState<SampleState>>();
        var dispatcher = provider.GetRequiredService<IDispatcher>();
        var startupWorks = await VerifyAsync(store, dispatcher, state, new StartupRequest(), ["startup-method"]);

        IServiceCollection module = new ServiceCollection();
        module.AddFluxorLibrary(options => options.ScanTypes(typeof(DynamicTypedEffect), typeof(DynamicMethods), typeof(DynamicStaticMethods)));
        await provider.LoadServiceDescriptors(module);
        provider.Build();
        var sameStore = ReferenceEquals(store, provider.GetRequiredService<IStore>());
        Console.WriteLine($"Same store after dynamic registration: {sameStore}");
        var typedWorks = await VerifyAsync(store, dispatcher, state, new TypedRequest(), ["dynamic-typed"]);
        var methodsWork = await VerifyAsync(store, dispatcher, state, new MethodRequest(), ["dynamic-method-first", "dynamic-method-second"]);
        var staticWorks = await VerifyAsync(store, dispatcher, state, new StaticRequest(), ["dynamic-static"]);
        var works = startupWorks && sameStore && typedWorks && methodsWork && staticWorks;
        if (works)
        {
            Console.WriteLine("PASS: startup, typed, instance and static method effects handled their actions.");
            return 0;
        }

        Console.Error.WriteLine("FAIL: dynamically scanned effect methods did not join the existing store.");
        return 1;
    }

    private static async Task<bool> VerifyAsync(IStore store, IDispatcher dispatcher, IState<SampleState> state, object action, string[] expected)
    {
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        void StateChanged(object? sender, EventArgs args)
        {
            if (expected.All(name => state.Value.Handled.Contains(name)))
            {
                completed.TrySetResult();
            }
        }
        state.StateChanged += StateChanged;
        EventHandler<Fluxor.Exceptions.UnhandledExceptionEventArgs> unhandled = (_, args) => completed.TrySetException(args.Exception);
        store.UnhandledException += unhandled;
        try
        {
            dispatcher.Dispatch(action);
            await completed.Task.WaitAsync(TimeSpan.FromSeconds(2));
            Console.WriteLine($"PASS: {string.Join(", ", expected)}");
            return true;
        }
        catch (TimeoutException)
        {
            Console.Error.WriteLine($"FAIL: no result for {string.Join(", ", expected.Where(name => !state.Value.Handled.Contains(name)))}");
            return false;
        }
        finally
        {
            state.StateChanged -= StateChanged;
            store.UnhandledException -= unhandled;
        }
    }
}

[FeatureState]
public sealed record SampleState
{
    public IReadOnlyList<string> Handled { get; init; } = [];
}

public sealed record StartupRequest;
public sealed record TypedRequest;
public sealed record MethodRequest;
public sealed record StaticRequest;
public sealed record Handled(string Name);

public static class SampleReducers
{
    [ReducerMethod]
    public static SampleState Record(SampleState state, Handled action)
    {
        return state with { Handled = [.. state.Handled, action.Name] };
    }
}

public sealed class EffectDependency
{
    public void Complete(IDispatcher dispatcher, string name)
    {
        dispatcher.Dispatch(new Handled(name));
    }
}

public sealed class InjectAttribute : Attribute;

public sealed class StartupMethods(EffectDependency dependency)
{
    [EffectMethod]
    public Task Handle(StartupRequest action, IDispatcher dispatcher)
    {
        dependency.Complete(dispatcher, "startup-method");
        return Task.CompletedTask;
    }
}

public sealed class DynamicTypedEffect : Effect<TypedRequest>
{
    [Inject] public EffectDependency Dependency { get; set; } = null!;

    public override Task HandleAsync(TypedRequest action, IDispatcher dispatcher)
    {
        Dependency.Complete(dispatcher, "dynamic-typed");
        return Task.CompletedTask;
    }
}

public sealed class DynamicMethods
{
    [Inject] public EffectDependency Dependency { get; set; } = null!;

    [EffectMethod]
    public Task First(MethodRequest action, IDispatcher dispatcher)
    {
        Dependency.Complete(dispatcher, "dynamic-method-first");
        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task Second(MethodRequest action, IDispatcher dispatcher)
    {
        Dependency.Complete(dispatcher, "dynamic-method-second");
        return Task.CompletedTask;
    }
}

public static class DynamicStaticMethods
{
    [EffectMethod]
    public static Task Handle(StaticRequest action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new Handled("dynamic-static"));
        return Task.CompletedTask;
    }
}
