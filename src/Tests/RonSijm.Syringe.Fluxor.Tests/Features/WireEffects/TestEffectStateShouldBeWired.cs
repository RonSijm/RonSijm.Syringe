using Fluxor;
using RonSijm.Syringe.Fluxor.Tests.Features.RestoreDispatchedStates;
using RonSijm.Syringe.Fluxor.Tests.Redux.CounterThroughEffectWithInjection;
using RonSijm.Syringe.Fluxor.Tests.Redux.ThroughEffectWithInjection;

namespace RonSijm.Syringe.Fluxor.Tests.Features.WireEffects;

public class TestEffectStateShouldBeWired
{
    [Fact]
    public async Task TestIncreaseTestCounter_Trough_IncreaseTestCounterAction()
    {
        var serviceProvider = await CreateServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();

        var counter = serviceProvider.GetRequiredService<IState<IncreaseTestCounterThroughEffectWithInjection_State>>();

        dispatcher.Dispatch(new IncreaseTestCounterThroughEffectWithInjection_Action());
        counter.Value.Count.Should().Be(1);

        dispatcher.Dispatch(new IncreaseTestCounterThroughEffectWithInjection_Action());
        counter.Value.Count.Should().Be(2);
    }

    private static async Task<SyringeServiceProvider> CreateServiceProvider()
    {
        var serviceProvider = new SyringeServiceProvider(options =>
        {
            options.WithAfterGetService(new PropertyInjectionAfterServiceExtension());
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