using RonSijm.Syringe.ServiceLookup;

namespace RonSijm.Syringe;

internal sealed class ServiceRegistrationCheckpoint
{
    private readonly Action _restore;

    public ServiceRegistrationCheckpoint(MicrosoftServiceProvider provider, IEnumerable<ServiceProviderEngineScope> scopes, List<AdditionProvider> additionalProviders)
    {
        var factory = provider.CallSiteFactory;
        var descriptors = factory.Descriptors.ToArray();
        var lookup = factory.DescriptorLookup.ToArray();
        var callSites = factory.CallSiteCache.ToArray();
        var values = callSites.Select(pair => pair.Value).Distinct().ToDictionary(site => site, site => site.Value);
        var accessors = provider.ServiceAccessors.ToArray();
        var overrides = additionalProviders.ToArray();
        var scopeStates = scopes.Prepend(provider.Root).Distinct().Select(scope => (Scope: scope, Services: scope.ResolvedServices.ToArray(), Disposables: scope.Disposables.Count)).ToArray();

        _restore = () =>
        {
            factory.Descriptors.Clear();
            factory.Descriptors.AddRange(descriptors);
            factory.DescriptorLookup.Clear();
            foreach (var pair in lookup)
            {
                factory.DescriptorLookup.Add(pair.Key, pair.Value);
            }
            factory.CallSiteCache.Clear();
            foreach (var pair in callSites)
            {
                pair.Value.Value = values[pair.Value];
                factory.CallSiteCache.TryAdd(pair.Key, pair.Value);
            }
            provider.ServiceAccessors.Clear();
            foreach (var pair in accessors)
            {
                provider.ServiceAccessors.TryAdd(pair.Key, pair.Value);
            }
            additionalProviders.Clear();
            additionalProviders.AddRange(overrides);
            foreach (var state in scopeStates)
            {
                state.Scope.ResolvedServices.Clear();
                foreach (var pair in state.Services)
                {
                    state.Scope.ResolvedServices.Add(pair.Key, pair.Value);
                }
            }
            var cleanupErrors = new List<Exception>();
            foreach (var state in scopeStates)
            {
                try
                {
                    state.Scope.RollbackDisposables(state.Disposables);
                }
                catch (Exception error)
                {
                    cleanupErrors.Add(error);
                }
            }
            if (cleanupErrors.Count > 0)
            {
                throw new AggregateException("Failed to dispose services created by the rejected registration.", cleanupErrors);
            }
        };
    }

    public void Restore()
    {
        _restore();
    }
}
