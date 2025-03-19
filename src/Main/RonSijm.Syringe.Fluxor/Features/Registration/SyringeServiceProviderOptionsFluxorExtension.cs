using Fluxor;
using Fluxor.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.DependencyInjection;

namespace RonSijm.Syringe;

public static class SyringeServiceProviderOptionsFluxorExtension
{
    public static void UseFluxor(this SyringeServiceProviderOptions providerOptions, Action<SyringeFluxorOptions> configure = null)
    {
        providerOptions.AfterBuildExtensions.Add(new WireFluxorAfterBuildExtension());

        if (configure == null)
        {
            return;
        }

        var fluxorOptions = new SyringeFluxorOptions(providerOptions.Services);
        configure(fluxorOptions);
        providerOptions.Services.AddSingleton<IEffect>(x => new UpdateEffect(x));

        fluxorOptions.WithLifetime(StoreLifetime.Singleton);
        
        if (fluxorOptions.DisableAddingFluxorItself)
        {
            return;
        }

        if (!fluxorOptions.DisableAddingStateDispatchRestore)
        {
            providerOptions.Services.AddSingleton<FeatureCache>();
            fluxorOptions.AddMiddleware<RestoreDispatchedStatesMiddleware>();
        }

        if (!fluxorOptions.DisableReduceAttributes)
        {
            providerOptions.AfterBuildExtensions.Add(new CreateReducersFromReduceIntoExtension());
        }

        providerOptions.Services.AddFluxorInternal(fluxorOptions);
    }
}