using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.DependencyInjection.WrapperFactories;
using RonSijm.Syringe.Extensions;

namespace RonSijm.Syringe.DependencyInjection.ServiceRegistration;

internal static class StoreRegistration
{
	public static void Register(
		IServiceCollection services,
		FluxorOptions options,
		FeatureClassInfo[] featureClassInfos,
		List<FeatureStateInfo> featureStateInfos,
		ReducerClassInfo[] reducerClassInfos,
		ReducerMethodInfo[] reducerMethodInfos,
		EffectClassInfo[] effectClassInfos,
		EffectMethodInfo[] effectMethodInfos)
	{
		FeatureRegistration.Register(
			services,
			featureClassInfos,
			featureStateInfos,
			reducerClassInfos,
			reducerMethodInfos,
			options);
		ReducerClassRegistration.Register(services, reducerClassInfos, options);
		ReducerMethodRegistration.Register(services, reducerMethodInfos, options);
		EffectClassRegistration.Register(services, effectClassInfos, options);
		EffectMethodRegistration.Register(services, effectMethodInfos, options);

		services.Add<IDispatcher, Dispatcher>(options);
		// Register IActionSubscriber as an alias to Store
		services.Add<IActionSubscriber>(serviceProvider => serviceProvider.GetService<Store>(), options);
		// Register IStore as an alias to Store
		services.Add<IStore>(serviceProvider => serviceProvider.GetService<Store>(), options);

		// Register a custom factory for building IStore that will inject all effects
		services.Add(typeof(Store), serviceProvider =>
		{
			var dispatcher = serviceProvider.GetService<IDispatcher>();
			var store = new Store(dispatcher);
			foreach (var featureClassInfo in featureClassInfos)
			{
				var feature = (IFeature)serviceProvider.GetService(featureClassInfo.FeatureInterfaceGenericType);
				store.AddFeature(feature);
			}

			foreach (var featureStateInfo in featureStateInfos)
			{
				var feature = (IFeature)serviceProvider.GetService(featureStateInfo.FeatureInterfaceGenericType);
				store.AddFeature(feature);
			}

			foreach (var effectClassInfo in effectClassInfos)
			{
				var effect = (IEffect)serviceProvider.GetService(effectClassInfo.ImplementingType);
				store.AddEffect(effect);
			}

			foreach (var effectMethodInfo in effectMethodInfos)
			{
				var effect = EffectWrapperFactory.Create(serviceProvider, effectMethodInfo);
				store.AddEffect(effect);
			}

			foreach (var middlewareType in options.MiddlewareTypes)
			{
				var middleware = (IMiddleware)serviceProvider.GetService(middlewareType);
				store.AddMiddleware(middleware);
			}

			return store;
		},
		options);

	}
}
