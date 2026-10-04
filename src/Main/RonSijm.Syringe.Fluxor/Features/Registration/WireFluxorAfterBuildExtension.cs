using Fluxor;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.DependencyInjection;
using RonSijm.Syringe.DependencyInjection.ServiceRegistration;
using RonSijm.Syringe.DependencyInjection.WrapperFactories;

namespace RonSijm.Syringe;

public class WireFluxorAfterBuildExtension : ISyringePreparedAfterBuildExtension, ISyringeBeforeBuildExtension
{
    private SyringeServiceProvider _serviceProvider;
    private readonly ConditionalWeakTable<List<ServiceDescriptor>, HashSet<IStore>> _preparedStores = new();
    private static readonly HashSet<Type> Infrastructure = [typeof(Store), typeof(IStore), typeof(IDispatcher), typeof(IActionSubscriber), typeof(IState<>), typeof(IStateSelection<,>), typeof(FluxorRegistrationTracker)];

    public void SetReference(SyringeServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void PrepareDescriptors(List<ServiceDescriptor> descriptors, bool isInitialBuild)
    {
        var existing = _serviceProvider.ServiceDescriptors;
        var modules = Modules(existing.Concat(descriptors)).ToArray();
        var definitions = new Dictionary<Type, object>();
        foreach (var feature in modules.SelectMany(module => module.Features))
        {
            if (definitions.TryGetValue(feature.StateType, out var definition) && !Equals(definition, feature.Definition))
            {
                throw new InvalidOperationException($"Conflicting Fluxor feature definitions for state '{feature.StateType.FullName}'.");
            }
            definitions[feature.StateType] = feature.Definition;
        }

        var lifetime = existing.Concat(descriptors).FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IStore))?.Lifetime;
        foreach (var module in modules.Where(module => module.Options.HasExplicitLifetime))
        {
            if (lifetime != null && module.Options.ServiceLifetime != lifetime)
            {
                throw new InvalidOperationException($"Fluxor module lifetime '{module.Options.ServiceLifetime}' must match the existing store lifetime '{lifetime}'.");
            }
        }
        var generatedTypes = modules.SelectMany(module => module.ServiceTypes).Concat(Infrastructure).ToHashSet();
        var accepted = existing.ToList();
        foreach (var descriptor in descriptors.ToArray())
        {
            if (modules.Length > 0 && accepted.Any(candidate => SameRegistration(candidate, descriptor)))
            {
                descriptors.Remove(descriptor);
                continue;
            }
            if (descriptor.IsKeyedService || !generatedTypes.Contains(descriptor.ServiceType))
            {
                accepted.Add(descriptor);
                continue;
            }
            if (accepted.Any(candidate => !candidate.IsKeyedService && candidate.ServiceType == descriptor.ServiceType))
            {
                descriptors.Remove(descriptor);
                continue;
            }
            if (lifetime != null && descriptor.Lifetime != lifetime)
            {
                var replacement = WithLifetime(descriptor, lifetime.Value);
                descriptors[descriptors.IndexOf(descriptor)] = replacement;
                accepted.Add(replacement);
            }
            else
            {
                accepted.Add(descriptor);
            }
        }
    }

    private static bool SameRegistration(ServiceDescriptor first, ServiceDescriptor second)
    {
        if (ReferenceEquals(first, second))
        {
            return true;
        }
        if (first.ServiceType != second.ServiceType || first.Lifetime != second.Lifetime || first.IsKeyedService != second.IsKeyedService)
        {
            return false;
        }
        if (first.IsKeyedService)
        {
            if (!Equals(first.ServiceKey, second.ServiceKey))
            {
                return false;
            }
            var same = (first.KeyedImplementationType != null && first.KeyedImplementationType == second.KeyedImplementationType)
                || (first.KeyedImplementationInstance != null && ReferenceEquals(first.KeyedImplementationInstance, second.KeyedImplementationInstance))
                || (first.KeyedImplementationFactory != null && Equals(first.KeyedImplementationFactory, second.KeyedImplementationFactory));
            return same;
        }
        var identical = (first.ImplementationType != null && first.ImplementationType == second.ImplementationType)
            || (first.ImplementationInstance != null && ReferenceEquals(first.ImplementationInstance, second.ImplementationInstance))
            || (first.ImplementationFactory != null && Equals(first.ImplementationFactory, second.ImplementationFactory));
        return identical;
    }

    private static ServiceDescriptor WithLifetime(ServiceDescriptor descriptor, ServiceLifetime lifetime)
    {
        if (descriptor.ImplementationInstance != null)
        {
            return descriptor;
        }
        if (descriptor.ImplementationFactory != null)
        {
            return new ServiceDescriptor(descriptor.ServiceType, descriptor.ImplementationFactory, lifetime);
        }
        return new ServiceDescriptor(descriptor.ServiceType, descriptor.ImplementationType, lifetime);
    }

    private static IEnumerable<FluxorModuleRegistration> Modules(IEnumerable<ServiceDescriptor> descriptors) =>
        descriptors.Where(descriptor => !descriptor.IsKeyedService).Select(descriptor => descriptor.ImplementationInstance).OfType<FluxorModuleRegistration>().Distinct();

    public void Process(List<ServiceDescriptor> descriptors, bool isInitialBuild)
    {
        Prepare(descriptors, isInitialBuild)();
    }

    public Action Prepare(List<ServiceDescriptor> descriptors, bool isInitialBuild)
    {
        var provider = _serviceProvider;
        var storeDescriptor = provider.ServiceDescriptors.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IStore));
        if (provider.IsRootScope && storeDescriptor?.Lifetime == ServiceLifetime.Scoped)
        {
            return () => { };
        }

        var store = provider.GetRequiredService<IStore>();
        if (!_preparedStores.GetOrCreateValue(descriptors).Add(store))
        {
            return () => { };
        }
        var tracker = provider.GetRequiredService<FluxorRegistrationTracker>();
        var registrations = tracker.GetStore(store);
        var modules = Modules(descriptors).ToArray();
        var features = store.Features.Values.ToDictionary(feature => feature.GetStateType());
        var names = store.Features.Keys.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
        var definitions = new Dictionary<Type, object>(registrations.Features);
        var effects = new HashSet<object>(registrations.Effects);
        var middlewareTypes = new HashSet<Type>(registrations.Middlewares);
        var commits = new List<Action>();

        foreach (var module in modules)
        {
            foreach (var info in module.Features)
            {
                if (definitions.TryGetValue(info.StateType, out var definition))
                {
                    if (!Equals(definition, info.Definition))
                    {
                        throw new InvalidOperationException($"Conflicting Fluxor feature definitions for state '{info.StateType}'.");
                    }
                    continue;
                }
                var feature = (IFeature)provider.GetRequiredService(info.ServiceType);
                if (!names.Add(feature.GetName()))
                {
                    throw new InvalidOperationException($"Fluxor feature name '{feature.GetName()}' is already registered.");
                }
                feature.GetState();
                features.Add(info.StateType, feature);
                definitions.Add(info.StateType, info.Definition);
                commits.Add(() => store.AddFeature(feature));
            }
        }

        var reducers = features.Values.ToDictionary(feature => feature, feature => new HashSet<object>(tracker.GetReducers(feature)));
        foreach (var module in modules)
        {
            foreach (var info in module.ReducerClasses)
            {
                AddReducer(info.StateType, FluxorModuleRegistration.ReducerKey(info), () => provider.GetRequiredService(info.ImplementingType));
            }
            foreach (var info in module.ReducerMethods)
            {
                AddReducer(info.StateType, FluxorModuleRegistration.ReducerKey(info), () => ReducerWrapperFactory.Create(provider, info));
            }
            foreach (var info in module.EffectClasses)
            {
                if (effects.Add(info.ImplementingType))
                {
                    var effect = (IEffect)provider.GetRequiredService(info.ImplementingType);
                    commits.Add(() => store.AddEffect(effect));
                }
            }
            foreach (var info in module.EffectMethods)
            {
                if (effects.Add(FluxorModuleRegistration.EffectKey(info)))
                {
                    var effect = EffectWrapperFactory.Create(provider, info);
                    commits.Add(() => store.AddEffect(effect));
                }
            }
            foreach (var type in module.MiddlewareTypes)
            {
                if (middlewareTypes.Add(type))
                {
                    var middleware = (IMiddleware)provider.GetRequiredService(type);
                    commits.Add(() => store.AddMiddleware(middleware));
                }
            }
        }

        foreach (var descriptor in descriptors.Where(descriptor => !descriptor.IsKeyedService && typeof(IEffect).IsAssignableFrom(descriptor.ServiceType)))
        {
            var effect = (IEffect)provider.GetRequiredService(descriptor.ServiceType);
            if (effects.Add(effect.GetType()))
            {
                commits.Add(() => store.AddEffect(effect));
            }
        }

        var hosts = modules.SelectMany(module => module.EffectMethods.Where(info => !info.MethodInfo.IsStatic).Select(info => info.HostClassType)
            .Concat(module.ReducerMethods.Where(info => !info.MethodInfo.IsStatic).Select(info => info.HostClassType)));
        foreach (var type in hosts.Concat(descriptors.Where(descriptor => typeof(IFeature).IsAssignableFrom(descriptor.ServiceType) || typeof(IEffect).IsAssignableFrom(descriptor.ServiceType) || typeof(IMiddleware).IsAssignableFrom(descriptor.ServiceType)).Select(descriptor => descriptor.ServiceType)).Distinct())
        {
            provider.GetRequiredService(type);
        }

        return () =>
        {
            foreach (var commit in commits)
            {
                commit();
            }
            foreach (var pair in definitions)
            {
                registrations.Features[pair.Key] = pair.Value;
            }
            registrations.Effects.UnionWith(effects);
            registrations.Middlewares.UnionWith(middlewareTypes);
            foreach (var pair in reducers)
            {
                tracker.GetReducers(pair.Key).UnionWith(pair.Value);
            }
        };

        void AddReducer(Type stateType, object key, Func<object> factory)
        {
            if (!features.TryGetValue(stateType, out var feature))
            {
                throw new InvalidOperationException($"No Fluxor feature is registered for reducer state '{stateType}'. Register its feature first or in the same module.");
            }
            if (!reducers[feature].Add(key))
            {
                return;
            }
            var reducer = factory();
            var method = FeatureRegistration.GetAddReducerMethod(typeof(IFeature<>).MakeGenericType(stateType));
            commits.Add(() => method.Invoke(feature, [reducer]));
        }
    }
}
