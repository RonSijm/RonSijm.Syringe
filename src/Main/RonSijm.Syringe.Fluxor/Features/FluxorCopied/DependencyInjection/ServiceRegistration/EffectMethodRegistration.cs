using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.Extensions;

namespace RonSijm.Syringe.DependencyInjection.ServiceRegistration;

internal static class EffectMethodRegistration
{
	public static void Register(
		IServiceCollection services,
		EffectMethodInfo[] effectMethodInfos,
		FluxorOptions options)
	{
		var hostClassTypes =
			effectMethodInfos
				.Select(x => x.HostClassType)
				.Where(t => !t.IsAbstract)
				.Distinct();

		foreach (var hostClassType in hostClassTypes)
			services.Add(hostClassType, options);
	}
}
