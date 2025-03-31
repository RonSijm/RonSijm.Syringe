using Fluxor;
using RonSijm.Syringe.Fluxor.Tests.Features.RestoreDispatchedStates;
using RonSijm.Syringe.Fluxor.Tests.Redux.TestReduceFrom;

namespace RonSijm.Syringe.Fluxor.Tests.Features;

public class TestReduceFrom
{
    [Fact]
    public async Task Test_DispatchMainModelCreatesChild()
    {
        var serviceProvider = await CreateServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        dispatcher.Dispatch(new ReduceFromMainModel { Child = new ReduceFromChildModel { Count = 42 } });

        var counter = serviceProvider.GetRequiredService<IState<ReduceFromChildModel>>();

        counter.Value.Count.Should().Be(42);
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