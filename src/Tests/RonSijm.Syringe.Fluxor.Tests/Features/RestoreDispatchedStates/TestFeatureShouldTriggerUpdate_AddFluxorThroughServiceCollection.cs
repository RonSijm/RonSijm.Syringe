using Fluxor;
using RonSijm.Syringe.Fluxor.Tests.Redux.TestReducerMethod;

namespace RonSijm.Syringe.Fluxor.Tests.Features.RestoreDispatchedStates;

public class TestFeatureShouldTriggerUpdate_AddFluxorThroughServiceCollection
{
    [Fact]
    public async Task TestIncreaseTestCounter_Trough_IncreaseTestCounterAction()
    {
        var serviceProvider = await CreateServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        dispatcher.Dispatch(new IncreaseTestCounterAction());

        var state = serviceProvider.GetRequiredService<IState<TestCounterState>>();

        state.Value.Count.Should().Be(1);
    }

    [Fact]
    public async Task TestIncreaseTestCounter_Trough_TestCounterStateDispatch()
    {
        var serviceProvider = await CreateServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        dispatcher.Dispatch(new TestCounterState { Count = 3 });

        var state = serviceProvider.GetRequiredService<IState<TestCounterState>>();
        state.Value.Count.Should().Be(3);
    }

    private static async Task<SyringeServiceProvider> CreateServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<FeatureCache>();

        var serviceProvider = new SyringeServiceProvider(serviceCollection, options =>
        {
            options.UseFluxor(fluxorOptions =>
            {
                fluxorOptions.ScanAssemblies(typeof(TestFeatureShouldTriggerUpdate_AddFluxorThroughServiceCollection).Assembly)
                    .AddMiddleware<RestoreDispatchedStatesMiddleware>();
            });
        });

        var store = serviceProvider.GetRequiredService<Store>();
        await store.InitializeAsync();

        return serviceProvider;
    }
}