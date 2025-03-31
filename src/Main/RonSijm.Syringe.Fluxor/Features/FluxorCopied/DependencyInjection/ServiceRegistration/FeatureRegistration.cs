using System.Reflection;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.DependencyInjection.WrapperFactories;
using RonSijm.Syringe.Extensions;

namespace RonSijm.Syringe.DependencyInjection.ServiceRegistration;

internal static class FeatureRegistration
{
    public static void Register(
        IServiceCollection services,
        FeatureClassInfo[] featureClassInfos,
        List<FeatureStateInfo> featureStateInfos,
        ReducerClassInfo[] reducerClassInfos,
        ReducerMethodInfo[] reducerMethodInfos,
        FluxorOptions options)
    {
        var reducerClassInfoByStateType =
            reducerClassInfos
            .GroupBy(x => x.StateType)
            .ToDictionary(x => x.Key);

        var reducerMethodInfoByStateType =
            reducerMethodInfos
                .GroupBy(x => x.StateType)
                .ToDictionary(x => x.Key);

        RegisterFeatureClassInfos(
            services,
            featureClassInfos,
            reducerClassInfoByStateType,
            reducerMethodInfoByStateType,
            options);

        RegisterStateInfos(
            services,
            featureStateInfos,
            reducerClassInfoByStateType,
            reducerMethodInfoByStateType,
            options);
    }

    private static void RegisterFeatureClassInfos(IServiceCollection services, FeatureClassInfo[] featureClassInfos, Dictionary<Type, IGrouping<Type, ReducerClassInfo>> reducerClassInfoByStateType, Dictionary<Type, IGrouping<Type, ReducerMethodInfo>> reducerMethodInfoByStateType, FluxorOptions options)
    {
        foreach (var info in featureClassInfos)
        {
            reducerClassInfoByStateType.TryGetValue(
                info.StateType,
                out var reducerClassInfosForStateType);

            reducerMethodInfoByStateType.TryGetValue(
                info.StateType,
                out var reducerMethodInfosForStateType);

            // Register the implementing type so we can get an instance from the service provider
            services.Add(info.ImplementingType, options);

            // Register a factory for the feature's interface
            services.Add(info.FeatureInterfaceGenericType, serviceProvider =>
            {
                // Create an instance of the implementing type
                var featureInstance =
                    (IFeature)serviceProvider.GetService(info.ImplementingType);

                AddReducers(
                    serviceProvider,
                    featureInstance,
                    reducerClassInfosForStateType,
                    reducerMethodInfosForStateType);

                return featureInstance;
            },
            options);
        }
    }

    public static MethodInfo GetAddReducerMethod(Type featureImplementingType)
    {
        var addReducerMethodName = nameof(IFeature<object>.AddReducer);
        var featureAddReducerMethodInfo =
            featureImplementingType.GetMethod(addReducerMethodName);
        return featureAddReducerMethodInfo;

    }

    private static void RegisterStateInfos(
        IServiceCollection services,
        List<FeatureStateInfo> featureStateInfos,
        Dictionary<Type, IGrouping<Type, ReducerClassInfo>> reducerClassInfoByStateType,
        Dictionary<Type, IGrouping<Type, ReducerMethodInfo>> reducerMethodInfoByStateType,
        FluxorOptions options)
    {
        foreach (var info in featureStateInfos)
        {
            reducerClassInfoByStateType.TryGetValue(
                info.StateType,
                out var reducerClassInfosForStateType);

            reducerMethodInfoByStateType.TryGetValue(
                info.StateType,
                out var reducerMethodInfosForStateType);

            // Register a factory for the feature's interface
            services.Add(info.FeatureInterfaceGenericType, serviceProvider =>
                {
                    IFeature featureInstance;

                    // Create an instance of the implementing type
                    if (info.FeatureWrapperGenericType.FullName.StartsWith("RonSijm.Syringe.UpdateChildrenFeatureStateWrapper"))
                    {
                        var featureConstructor = info.FeatureWrapperGenericType.GetConstructor(new[] { typeof(FeatureStateInfo), typeof(IDispatcher) });
                        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();

                        featureInstance = (IFeature)featureConstructor.Invoke(new object[] { info, dispatcher });
                    }
                    else
                    {
                        var featureConstructor = info.FeatureWrapperGenericType.GetConstructor(new[] { typeof(FeatureStateInfo) });
                        featureInstance = (IFeature)featureConstructor.Invoke(new object[] { info });
                    }

                    AddReducers(
                        serviceProvider,
                        featureInstance,
                        reducerClassInfosForStateType,
                        reducerMethodInfosForStateType);

                    return featureInstance;
                },
            options);
        }
    }

    private static void AddReducers(
        IServiceProvider serviceProvider,
        IFeature featureInstance,
        IEnumerable<ReducerClassInfo> reducerClassInfosForStateType,
        IEnumerable<ReducerMethodInfo> reducerMethodInfosForStateType)
    {
        var featureAddReducerMethodInfo = GetAddReducerMethod(featureInstance.GetType());

        if (reducerClassInfosForStateType is not null)
        {
            foreach (var reducerClass in reducerClassInfosForStateType)
            {
                var reducerInstance = serviceProvider.GetService(reducerClass.ImplementingType);
                featureAddReducerMethodInfo.Invoke(featureInstance, new[] { reducerInstance });
            }
        }

        if (reducerMethodInfosForStateType is not null)
        {
            foreach (var reducerMethodInfo in reducerMethodInfosForStateType)
            {
                var reducerWrapperInstance = ReducerWrapperFactory.Create(serviceProvider, reducerMethodInfo);
                featureAddReducerMethodInfo.Invoke(featureInstance, new[] { reducerWrapperInstance });
            }
        }
    }
}
