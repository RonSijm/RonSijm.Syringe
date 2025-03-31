using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.Extensions;

namespace RonSijm.Syringe.DependencyInjection.ServiceRegistration;

internal static class ReducerMethodRegistration
{
	public static void Register(
		IServiceCollection services,
		ReducerMethodInfo[] reducerMethodInfos,
		FluxorOptions options)
	{
		var hostClassTypes =
			reducerMethodInfos
				.Select(x => x.HostClassType)
				.Where(t => !t.IsAbstract)
				.Distinct();

		foreach (var hostClassType in hostClassTypes)
			services.Add(hostClassType, options);
	}
}
