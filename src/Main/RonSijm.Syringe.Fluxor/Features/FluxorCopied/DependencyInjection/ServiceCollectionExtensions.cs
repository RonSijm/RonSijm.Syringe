using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.Extensions;

namespace RonSijm.Syringe.DependencyInjection;

/// <summary>
/// Extensions for <see cref="IServiceCollection"/>
/// </summary>
public static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddFluxorInternal(this IServiceCollection services, Action<SyringeFluxorOptions> config = null)
    {
		var options = new SyringeFluxorOptions(services);
        config?.Invoke(options);
        return AddFluxorInternal(services, options);
    }


    public static IServiceCollection AddFluxorInternal(this IServiceCollection services, SyringeFluxorOptions options)
	{
		// Register all middleware types with dependency injection
        foreach (var middlewareType in options.MiddlewareTypes)
        {
            services.Add(middlewareType, options);
        }

		var scanIncludeList = options.MiddlewareTypes.Select(t => new AssemblyScanSettings(t.Assembly, t.Namespace));

		ReflectionScanner.Scan(
			options: options,
			services: services,
			assembliesToScan: options.AssembliesToScan,
			typesToScan: options.TypesToScan,
			scanIncludeList: scanIncludeList);
		services.Add(typeof(IState<>), typeof(State<>), options);
		services.AddTransient(typeof(IStateSelection<,>), typeof(StateSelection<,>));

		return services;
	}
}