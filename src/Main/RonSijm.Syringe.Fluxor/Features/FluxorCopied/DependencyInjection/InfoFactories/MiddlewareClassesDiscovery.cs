using System.Reflection;
using Fluxor;

namespace RonSijm.Syringe.DependencyInjection.InfoFactories;

internal static class MiddlewareClassesDiscovery
{
	internal static AssemblyScanSettings[] FindMiddlewareLocations(IEnumerable<Assembly> assembliesToScan)
	=>
		assembliesToScan
			.SelectMany(x => x.GetTypes().Where(t => t.GetInterfaces().Any(i => i == typeof(IMiddleware))))
			.Select(x => new AssemblyScanSettings(x.Assembly, x.Namespace))
			.Distinct()
			.ToArray();
}
