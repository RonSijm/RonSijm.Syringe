using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.DependencyInjection;

namespace RonSijm.Syringe;

public class SyringeFluxorOptions(IServiceCollection services) : FluxorOptions(services)
{
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

    public SyringeFluxorOptions AddNativeExtension(Action<NativeFluxorOptions> config)
    {
        var newServices = new ServiceCollection();
        var options = new NativeFluxorOptions(services);
        config.Invoke(options);

        var optionsType = typeof(NativeFluxorOptions);

        var assembliesToScan = optionsType.GetProperty("AssembliesToScan", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(options);
        var typesToScan = optionsType.GetProperty("TypesToScan", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(options) as Type[];
        var middlewareTypes = optionsType.GetField("MiddlewareTypes", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(options) as Type[];

        TypesToScan.AddRange(typesToScan);
        MiddlewareTypes.AddRange(middlewareTypes);

        return this;
    }
}