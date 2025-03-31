using Fluxor;
using RonSijm.Syringe.Fluxor.Tests.Features.RestoreDispatchedStates;
using RonSijm.Syringe.Fluxor.Tests.Redux.TestReducerMethod;

namespace RonSijm.Syringe.Fluxor.Tests.Features.ReduceInto;

public class TestFeatureShouldTriggerUpdate_OnViewModel
{
    [Fact]
    public async Task TestIncreaseTestCounter_Trough_IncreaseTestCounterAction()
    {
        var serviceProvider = await CreateServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        dispatcher.Dispatch(new IncreaseTestCounterAction());

        var counter = serviceProvider.GetRequiredService<IState<TestCounterState>>();
        var viewModel = serviceProvider.GetRequiredService<IState<TestCounterViewModel>>();

        counter.Value.Count.Should().Be(1);
        viewModel.Value.Counter.Count.Should().Be(1);
    }

    [Fact]
    public async Task TestIncreaseTestCounter_Trough_TestCounterStateDispatch()
    {
        var serviceProvider = await CreateServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        dispatcher.Dispatch(new TestCounterState { Count = 3 });

        var counter = serviceProvider.GetRequiredService<IState<TestCounterState>>();
        var viewModel = serviceProvider.GetRequiredService<IState<TestCounterViewModel>>();

        counter.Value.Count.Should().Be(3);
        viewModel.Value.Counter.Count.Should().Be(3);
    }

    private static async Task<SyringeServiceProvider> CreateServiceProvider()
    {
        var serviceProvider = new SyringeServiceProvider(options =>
        {
            options.UseFluxor(fluxor =>
            {
                fluxor.ScanAssemblies<TestFeatureShouldTriggerUpdate_AddFluxorThroughExtension>();
            });
        });

        var store = serviceProvider.GetRequiredService<Store>();
        await store.InitializeAsync();

        return serviceProvider;
    }
}