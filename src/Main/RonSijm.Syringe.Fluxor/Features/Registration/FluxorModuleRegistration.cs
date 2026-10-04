using Fluxor;
using RonSijm.Syringe.DependencyInjection;

namespace RonSijm.Syringe;

internal sealed class FluxorModuleRegistration(FluxorOptions options, FeatureClassInfo[] featureClasses, List<FeatureStateInfo> featureStates, ReducerClassInfo[] reducerClasses, ReducerMethodInfo[] reducerMethods, EffectClassInfo[] effectClasses, EffectMethodInfo[] effectMethods)
{
    internal FluxorOptions Options { get; } = options;
    internal FeatureClassInfo[] FeatureClasses { get; } = featureClasses;
    internal List<FeatureStateInfo> FeatureStates { get; } = featureStates;
    internal ReducerClassInfo[] ReducerClasses { get; } = reducerClasses;
    internal ReducerMethodInfo[] ReducerMethods { get; } = reducerMethods;
    internal EffectClassInfo[] EffectClasses { get; } = effectClasses;
    internal EffectMethodInfo[] EffectMethods { get; } = effectMethods;
    internal Type[] MiddlewareTypes { get; } = options.MiddlewareTypes.ToArray();

    internal IEnumerable<(Type StateType, Type ServiceType, object Definition)> Features =>
        FeatureClasses.Select(info => (info.StateType, info.FeatureInterfaceGenericType, (object)info.ImplementingType))
            .Concat(FeatureStates.Select(info => (info.StateType, info.FeatureInterfaceGenericType, (object)(info.FeatureWrapperGenericType, info.FeatureStateAttribute.Name, info.FeatureStateAttribute.CreateInitialStateMethodName, info.FeatureStateAttribute.MaximumStateChangedNotificationsPerSecond))));

    internal IEnumerable<Type> ServiceTypes =>
        Features.Select(info => info.ServiceType)
            .Concat(FeatureClasses.Select(info => info.ImplementingType))
            .Concat(ReducerClasses.Select(info => info.ImplementingType))
            .Concat(ReducerMethods.Where(info => !info.MethodInfo.IsStatic).Select(info => info.HostClassType))
            .Concat(EffectClasses.Select(info => info.ImplementingType))
            .Concat(EffectMethods.Where(info => !info.MethodInfo.IsStatic).Select(info => info.HostClassType))
            .Concat(MiddlewareTypes);

    internal static object ReducerKey(ReducerClassInfo info) => (info.ImplementingType, info.StateType);
    internal static object ReducerKey(ReducerMethodInfo info) => (info.HostClassType, info.MethodInfo);
    internal static object EffectKey(EffectMethodInfo info) => (info.HostClassType, info.MethodInfo);
}
