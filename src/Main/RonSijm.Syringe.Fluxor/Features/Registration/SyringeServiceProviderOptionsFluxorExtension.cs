using Fluxor;
using Microsoft.Extensions.DependencyInjection;
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

        if (!fluxorOptions.DisablePropertyInjection)
        {
            providerOptions.WithAfterGetService<PropertyInjectionAfterServiceExtension>();
        }

        providerOptions.Services.AddSingleton<IEffect>(x => new UpdateEffect(x));

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

        providerOptions.Services.AddFluxorLibrary(fluxorOptions);
    }
}