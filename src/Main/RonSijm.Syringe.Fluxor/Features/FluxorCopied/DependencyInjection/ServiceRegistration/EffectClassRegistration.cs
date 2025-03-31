using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.Extensions;

namespace RonSijm.Syringe.DependencyInjection.ServiceRegistration;

internal static class EffectClassRegistration
{
	public static void Register(
		IServiceCollection services,
		IEnumerable<EffectClassInfo> effectClassInfos,
		FluxorOptions options)
	{
		foreach (var effectClassInfo in effectClassInfos)
			services.Add(effectClassInfo.ImplementingType, options);
	}
}
