using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RonSijm.Syringe.DependencyInjection;

namespace RonSijm.Syringe;

public static class SyringeServiceProviderOptionsFluxorExtension
{
    public static void UseFluxor(this SyringeServiceProviderOptions providerOptions, Action<SyringeFluxorOptions> configure = null)
    {
        var fluxorOptions = new SyringeFluxorOptions(providerOptions.Services);
        configure?.Invoke(fluxorOptions);

        UseFluxor(providerOptions, fluxorOptions);
    }

    public static void UseFluxor(this SyringeServiceProviderOptions providerOptions, SyringeFluxorOptions fluxorOptions)
    {
        providerOptions.WithAfterBuildExtension<WireFluxorAfterBuildExtension>();
        providerOptions.Services.TryAddSingleton<FluxorRegistrationTracker>();

        if (!fluxorOptions.DisablePropertyInjection)
        {
            providerOptions.WithAfterGetService<PropertyInjectionAfterServiceExtension>();
        }

        providerOptions.Services.Add(new ServiceDescriptor(typeof(IEffect), x => new UpdateEffect(x), fluxorOptions.ServiceLifetime));

        if (fluxorOptions.DisableAddingFluxorItself)
        {
            return;
        }

        if (!fluxorOptions.DisableAddingStateDispatchRestore)
        {
            providerOptions.Services.Add(new ServiceDescriptor(typeof(FeatureCache), typeof(FeatureCache), fluxorOptions.ServiceLifetime));
            fluxorOptions.AddMiddleware<RestoreDispatchedStatesMiddleware>();
        }

        if (!fluxorOptions.DisableReduceAttributes)
        {
            providerOptions.AfterBuildExtensions.Add(new CreateReducersFromReduceIntoExtension());
        }

        providerOptions.Services.AddFluxorLibrary(fluxorOptions);
    }
}