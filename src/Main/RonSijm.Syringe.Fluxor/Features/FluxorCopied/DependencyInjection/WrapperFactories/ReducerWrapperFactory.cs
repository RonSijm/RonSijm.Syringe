using RonSijm.Syringe.DependencyInjection.Wrappers;

namespace RonSijm.Syringe.DependencyInjection.WrapperFactories;

internal static class ReducerWrapperFactory
{
	internal static object Create(
		IServiceProvider serviceProvider,
		ReducerMethodInfo info)
	{
		var stateType = info.StateType;
		var actionType = info.ActionType;

		var hostClassType = info.HostClassType;
		var reducerHostInstance = info.MethodInfo.IsStatic
			? null
			: serviceProvider.GetService(hostClassType);

		var classGenericType = typeof(ReducerWrapper<,>).MakeGenericType(stateType, actionType);
		var result = Activator.CreateInstance(
			classGenericType,
			reducerHostInstance,
			info);
		return result;
	}
}
