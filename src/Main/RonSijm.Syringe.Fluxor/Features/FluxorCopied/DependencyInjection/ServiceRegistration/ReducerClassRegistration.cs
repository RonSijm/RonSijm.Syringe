using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.Extensions;

namespace RonSijm.Syringe.DependencyInjection.ServiceRegistration;

internal static class ReducerClassRegistration
{
	public static void Register(
		IServiceCollection services,
		ReducerClassInfo[] reducerClassInfos,
		FluxorOptions options)
	{
		foreach (var reducerClassInfo in reducerClassInfos)
			services.Add(serviceType: reducerClassInfo.ImplementingType, options: options);
	}
}
