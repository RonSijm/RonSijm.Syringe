using System.Runtime.CompilerServices;
using Fluxor;

namespace RonSijm.Syringe;

internal sealed class FluxorRegistrationTracker
{
    private readonly ConditionalWeakTable<IStore, StoreRegistrations> _stores = new();
    private readonly ConditionalWeakTable<IFeature, HashSet<object>> _reducers = new();

    internal StoreRegistrations GetStore(IStore store) => _stores.GetOrCreateValue(store);
    internal HashSet<object> GetReducers(IFeature feature) => _reducers.GetOrCreateValue(feature);

    internal void RecordInitialStore(IStore store, FluxorModuleRegistration module)
    {
        var registrations = GetStore(store);
        foreach (var feature in module.Features)
        {
            registrations.Features[feature.StateType] = feature.Definition;
        }
        registrations.Effects.UnionWith(module.EffectClasses.Select(info => (object)info.ImplementingType));
        registrations.Effects.UnionWith(module.EffectMethods.Select(FluxorModuleRegistration.EffectKey));
        registrations.Middlewares.UnionWith(module.MiddlewareTypes);
    }

    internal sealed class StoreRegistrations
    {
        internal Dictionary<Type, object> Features { get; } = new();
        internal HashSet<object> Effects { get; } = new();
        internal HashSet<Type> Middlewares { get; } = new();
    }
}
