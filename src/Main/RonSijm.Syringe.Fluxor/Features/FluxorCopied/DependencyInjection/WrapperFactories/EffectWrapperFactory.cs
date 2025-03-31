using Fluxor;
using RonSijm.Syringe.DependencyInjection.Wrappers;

namespace RonSijm.Syringe.DependencyInjection.WrapperFactories;

internal static class EffectWrapperFactory
{
	internal static IEffect Create(
		IServiceProvider serviceProvider,
		EffectMethodInfo info)
	{
		var actionType = info.ActionType;

		var hostClassType = info.HostClassType;
		var effectHostInstance = info.MethodInfo.IsStatic
			? null
			: serviceProvider.GetService(hostClassType);

		var classGenericType = typeof(EffectWrapper<>).MakeGenericType(actionType);
		var result = (IEffect)Activator.CreateInstance(
			classGenericType,
			effectHostInstance,
			info);
		return result;
	}
}
