using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.DependencyInjection;

namespace RonSijm.Syringe;

public class SyringeFluxorOptions : FluxorOptions
{
    public SyringeFluxorOptions(IServiceCollection services) : base(services)
    {
        SetDefaultLifetime(StoreLifetime.Singleton);
    }

    public bool DisableAddingFluxorItself { get; set; }
    public bool DisableAddingStateDispatchRestore { get; set; }
    public bool DisableReduceAttributes { get; set; }
    public bool DisableUpdateChildrenFeature { get; set; }
    public bool DisablePropertyInjection { get; set; }

    public SyringeFluxorOptions ScanAssemblies<T>()
    {
        ScanAssemblies(typeof(T).Assembly);
        return this;
    }

    public delegate void NativeExtensionConfiguration(SyringeFluxorOptions options);

    public SyringeFluxorOptions AddNativeExtension(NativeExtensionConfiguration config)
    {
        config(this);

        // Native extensions register middleware through the public service collection.
        foreach (var type in Services.Select(descriptor => descriptor.ServiceType).Where(type => typeof(IMiddleware).IsAssignableFrom(type)).Distinct())
        {
            if (!MiddlewareTypes.Contains(type))
            {
                MiddlewareTypes.Add(type);
            }
        }

        return this;
    }
}