using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.Scope;
using RonSijm.Syringe.ServiceLookup;

namespace RonSijm.Syringe;

public class SyringeServiceProvider : IKeyedServiceProvider, IDisposable, IAsyncDisposable
{
    private IServiceProvider ScopedProvider { get; set; }
    private MicrosoftServiceProvider RootProvider { get; set; }
    public SyringeServiceProviderOptions Options { get; private set; }
    internal IServiceCollection Services { get; private set; }
    internal List<ServiceDescriptor> NewServices { get; private set; }
    private List<ScopeWrapper> Scopes { get; } = new();

    public SyringeServiceProvider(SyringeServiceProviderOptions options = null)
    {
        Construct(new SyringeServiceCollection(), options);
    }

    private SyringeServiceProvider()
    {
    }

    public ScopeWrapper CreateScope()
    {
        var scoped = ConstructScoped();
        var scopeWrapper = new ScopeWrapper(scoped);
        Scopes.Add(scopeWrapper);

        return scopeWrapper;
    }

    private SyringeServiceProvider ConstructScoped()
    {
        var scope = RootProvider.CreateScope();
        var scopedProvider = scope.ServiceProvider;

        var scoped = new SyringeServiceProvider()
        {
            Options = Options,
            NewServices = NewServices,
            Services = Services,
            RootProvider = RootProvider,
            ScopedProvider = scopedProvider
        };
        return scoped;
    }

    public SyringeServiceProvider(Action<SyringeServiceProviderOptions> options)
    {
        var optionsModel = new SyringeServiceProviderOptions();
        options?.Invoke(optionsModel);

        Construct(new SyringeServiceCollection(), optionsModel);
    }

    public SyringeServiceProvider(IServiceCollection collection, Action<SyringeServiceProviderOptions> options)
    {
        var optionsModel = new SyringeServiceProviderOptions();
        options?.Invoke(optionsModel);

        Construct(collection, optionsModel);
    }

    public SyringeServiceProvider(IServiceCollection collection, SyringeServiceProviderOptions options = null)
    {
        Construct(collection, options);
    }

    private void Construct(IServiceCollection collection, SyringeServiceProviderOptions options)
    {
        Options = options ?? new SyringeServiceProviderOptions();
        Options.ServiceProviderOptions ??= new ServiceProviderOptions { RegisterServiceScopeFactory = false };
        Services = collection;
        NewServices = [];

        foreach (var item in collection)
        {
            NewServices.Add(item);
        }

        RegisterSelf(collection);

        if (Options.Services != null)
        {
            var collectionFromOptions = Options.Services.BuildServiceCollection();

            foreach (var collectionFromOption in collectionFromOptions)
            {
                collection.Add(collectionFromOption);
                NewServices.Add(collectionFromOption);
            }
        }

        foreach (var extension in Options.AfterGetServiceExtensions)
        {
            extension.SetReference(this);
        }

        foreach (var extension in Options.AfterBuildExtensions)
        {
            extension.SetReference(this);
        }

        if (Options.BuildOnConstruct)
        {
            BuildInitial();
        }
    }

    private void RegisterSelf(IServiceCollection collection)
    {
        collection.AddScoped(typeof(Optional<>), typeof(Optional<>));
        collection.AddSingleton<IServiceScopeFactory>(_ => new SyringeServiceScopeFactory(this));
        collection.AddSingleton<IServiceProvider>(this);
        collection.AddSingleton(this);
        Options.AdditionalProviders.Add(new SingletonProvider(typeof(IServiceProvider), this));
    }

    public virtual object GetService(Type serviceType)
    {
        if (TryGetServiceFromOverride(serviceType, out var value))
        {
            Options.AfterGetServiceExtensions.ForEach(x => x.Decorate(serviceType, value));
            return value;
        }

        var service = GetServiceWithoutExtensions(serviceType);

        if (service == null)
        {
            return null;
        }

        Options.AfterGetServiceExtensions.ForEach(x => x.Decorate(serviceType, service));

        TryAddDescriptorToCache(serviceType, service);

        return service;
    }

    public void TryAddDescriptorToCache(Type serviceType, object service)
    {
        var descriptor = GetDescriptor(serviceType);

        if (descriptor.Value?.Cache is { Location: CallSiteResultCacheLocation.Root })
        {
            Options.AdditionalProviders.Add(new SingletonProvider(serviceType, service));
        }
    }

    internal KeyValuePair<ServiceCacheKey, ServiceCallSite> GetDescriptor(Type serviceType)
    {
        var descriptor = RootProvider.CallSiteFactory.CallSiteCache.FirstOrDefault(x => x.Key.ServiceIdentifier.ServiceType == serviceType);
        return descriptor;
    }

    public object GetServiceWithoutExtensions(Type serviceType)
    {
        // TODO: Don't know how to fix scope.
        // Can't even reproduce broken scope in test...
        //return RootProvider.GetService(serviceType);

        if (ScopedProvider == null)
        {
            return RootProvider.GetService(serviceType);
        }

        var serviceIdentifier = RootProvider.GetServiceIdentifier(serviceType);
        var serviceAccessor = RootProvider.GetServiceAccessor(serviceIdentifier);

        if (serviceAccessor?.CallSite?.Cache is { Location: CallSiteResultCacheLocation.Root })
        {
            return RootProvider.GetService(serviceType);
        }

        return ScopedProvider.GetService(serviceType);
    }

    public bool TryGetServiceFromOverride(Type serviceType, out object value)
    {
        return TryGetServiceFromOverride(Options.AdditionalProviders, serviceType, out value);
    }

    public bool TryGetServiceFromOverride(List<AdditionProvider> providers, Type serviceType, out object value)
    {
        foreach (var typeFunctionOverride in providers)
        {
            if (!typeFunctionOverride.IsMatch(serviceType))
            {
                continue;
            }

            var result = typeFunctionOverride.Create(serviceType, this);
            {
                value = result;
                return true;
            }
        }

        value = null;
        return false;
    }

    public async Task<List<ServiceDescriptor>> LoadServiceDescriptors(IServiceCollection serviceCollection)
    {
        var loadedServiceDescriptor = new List<ServiceDescriptor>();

        foreach (var serviceDescriptor in serviceCollection)
        {
            RegisterServiceDescriptor(serviceDescriptor, loadedServiceDescriptor);
        }

        return loadedServiceDescriptor;
    }

    public async Task<List<ServiceDescriptor>> LoadServiceDescriptors(IAsyncEnumerable<ServiceDescriptor> serviceDescriptors)
    {
        var loadedServiceDescriptor = new List<ServiceDescriptor>();

        await foreach (var serviceDescriptor in serviceDescriptors)
        {
            RegisterServiceDescriptor(serviceDescriptor, loadedServiceDescriptor);
        }

        return loadedServiceDescriptor;
    }

    private void RegisterServiceDescriptor(ServiceDescriptor serviceDescriptor, List<ServiceDescriptor> loadedServiceDescriptor)
    {
        var existingService = Services.FirstOrDefault(x => x.ServiceType == serviceDescriptor.ServiceType);

        if (existingService != null)
        {
            return;
        }

        Services.Add(serviceDescriptor);
        NewServices.Add(serviceDescriptor);
        loadedServiceDescriptor.Add(serviceDescriptor);
    }

    private void BuildInitial()
    {
        RootProvider = Options.ServiceProviderBuilder == null ?
            Services.BuildServiceProvider(Options.ServiceProviderOptions) :
            Options.ServiceProviderBuilder(Services);

        var newServices = NewServices.ToList();
        NewServices.Clear();

        DoAfterBuild(newServices, true);
    }

    public void Build()
    {
        if (RootProvider == null)
        {
            BuildInitial();
            return;
        }

        var newServices = NewServices.ToList();
        NewServices.Clear();

        // Add the new descriptors to the cache
        RootProvider.CallSiteFactory.AddDescriptors(newServices);

        // Remove cached lookups for the new services, because if they were resolved before they were added, they're cached as null.
        foreach (var identifier in newServices.Select(ServiceIdentifier.FromDescriptor))
        {
            RootProvider.ServiceAccessors.TryRemove(identifier, out _);
        }

        DoAfterBuild(newServices, false);
    }

    private void DoAfterBuild(List<ServiceDescriptor> newServices, bool isInitialBuild)
    {
        Options.AfterBuildExtensions.ForEach(x => x.Process(newServices, isInitialBuild));

        // Note: We intentionally do NOT replace the scoped providers when rebuilding.
        // The existing scoped providers share the same RootProvider which has been updated
        // with the new service descriptors. Replacing the scoped providers would create
        // new instances of scoped services, breaking event subscriptions and other
        // references to the old instances (e.g., RadzenDialog subscribes to DialogService.OnOpen,
        // and replacing the scoped provider would give new components a different DialogService instance).
    }

    public void Dispose()
    {
        if (RootProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        Options.AdditionalProviders.Clear();
    }

    public ValueTask DisposeAsync()
    {
        if (RootProvider is IAsyncDisposable disposable)
        {
            return disposable.DisposeAsync();
        }

        return ValueTask.CompletedTask;
    }

    public virtual object GetKeyedService(Type serviceType, object serviceKey)
    {
        return RootProvider.GetKeyedService(serviceType, serviceKey);
    }

    public virtual object GetRequiredKeyedService(Type serviceType, object serviceKey)
    {
        return RootProvider.GetRequiredKeyedService(serviceType, serviceKey);
    }

    public void DisposeScope(ScopeWrapper scope)
    {
        Scopes.Remove(scope);
        var disposable = scope.ServiceProvider.ScopedProvider as IDisposable;
        disposable?.Dispose();
    }
}