using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.Scope;
using RonSijm.Syringe.ServiceLookup;

namespace RonSijm.Syringe;

public class SyringeServiceProvider : IKeyedServiceProvider, IDisposable, IAsyncDisposable
{
    private readonly object _registrationLock = new();
    private readonly ReaderWriterLockSlim _registrationGate = new(LockRecursionPolicy.SupportsRecursion);
    private SyringeServiceProvider _registrationOwner;
    private Exception _registrationFailure;
    private SyringeServiceProvider RegistrationOwner => _registrationOwner ?? this;
    public bool IsRootScope => ScopedProvider == null;
    public IReadOnlyList<ServiceDescriptor> ServiceDescriptors => RootProvider?.CallSiteFactory.Descriptors.ToArray() ?? [];
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
        RegistrationOwner._registrationGate.EnterReadLock();
        try
        {
            return CreateScopeInternal();
        }
        finally
        {
            RegistrationOwner._registrationGate.ExitReadLock();
        }
    }

    private ScopeWrapper CreateScopeInternal()
    {
        lock (RegistrationOwner._registrationLock)
        {
            RegistrationOwner.EnsureRegistrationHealthy();
            var scoped = ConstructScoped();
            var scopeWrapper = new ScopeWrapper(scoped);
            RegistrationOwner.Scopes.Add(scopeWrapper);
            try
            {
                scoped.DoAfterBuild(ServiceDescriptors.ToList(), true);
                return scopeWrapper;
            }
            catch
            {
                RegistrationOwner.DisposeScope(scopeWrapper);
                throw;
            }
        }
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
            ScopedProvider = scopedProvider,
            _registrationOwner = RegistrationOwner
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

        if (Options.ValidateOnBuild)
        {
            Options.ServiceProviderOptions.ValidateOnBuild = true;
        }

        if (Options.ValidateScopes)
        {
            Options.ServiceProviderOptions.ValidateScopes = true;
        }

        foreach (var validatorProvider in Options.AfterGetServiceExtensions.OfType<IProvideCallSiteValidator>())
        {
            Options.ServiceProviderOptions.AdditionalCallSiteValidatorFactories.Add(validatorProvider.CreateValidator);
        }

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
        RegistrationOwner._registrationGate.EnterReadLock();
        try
        {
            return GetServiceInternal(serviceType);
        }
        finally
        {
            RegistrationOwner._registrationGate.ExitReadLock();
        }
    }

    private object GetServiceInternal(Type serviceType)
    {
        ObjectDisposedException.ThrowIf(RootProvider?.IsDisposed() == true || ScopedProvider is ServiceProviderEngineScope { Disposed: true }, this);
        RegistrationOwner.EnsureRegistrationHealthy();
        if (serviceType == typeof(IServiceProvider) || serviceType == typeof(SyringeServiceProvider))
        {
            return this;
        }
        if (TryGetServiceFromOverride(serviceType, out var value))
        {
            Decorate(serviceType, value);
            return value;
        }

        var service = GetServiceWithoutExtensions(serviceType);

        if (service == null)
        {
            return null;
        }

        Decorate(serviceType, service);

        TryAddDescriptorToCache(serviceType, service);

        return service;
    }

    private void Decorate(Type serviceType, object service)
    {
        foreach (var extension in Options.AfterGetServiceExtensions)
        {
            var previous = RegistrationOwner;
            if (extension is SyringeServiceProviderAfterServiceExtensionBase contextual)
            {
                previous = contextual.SwapReference(this);
            }
            else
            {
                extension.SetReference(this);
            }
            try
            {
                extension.Decorate(serviceType, service);
            }
            finally
            {
                extension.SetReference(previous);
            }
        }
    }

    public void TryAddDescriptorToCache(Type serviceType, object service)
    {
        var descriptor = GetDescriptor(serviceType);

        if (descriptor.Value?.Cache is { Location: CallSiteResultCacheLocation.Root })
        {
            lock (Options.AdditionalProviders)
            {
                if (!Options.AdditionalProviders.Any(provider => provider is SingletonProvider && provider.IsMatch(serviceType)))
                {
                    Options.AdditionalProviders.Add(new SingletonProvider(serviceType, service));
                }
            }
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
        AdditionProvider[] snapshot;
        lock (providers)
        {
            snapshot = providers.ToArray();
        }
        foreach (var typeFunctionOverride in snapshot)
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

    public Task<List<ServiceDescriptor>> LoadServiceDescriptors(IServiceCollection serviceCollection)
    {
        return Task.FromResult(StageServiceDescriptors(serviceCollection.ToArray()));
    }

    public async Task<List<ServiceDescriptor>> LoadServiceDescriptors(IAsyncEnumerable<ServiceDescriptor> serviceDescriptors)
    {
        var collected = new List<ServiceDescriptor>();
        await foreach (var serviceDescriptor in serviceDescriptors)
        {
            collected.Add(serviceDescriptor);
        }
        return StageServiceDescriptors(collected);
    }

    private List<ServiceDescriptor> StageServiceDescriptors(IEnumerable<ServiceDescriptor> descriptors)
    {
        var loaded = new List<ServiceDescriptor>();
        RegistrationOwner._registrationGate.EnterWriteLock();
        try
        {
            RegistrationOwner.EnsureRegistrationHealthy();
            lock (RegistrationOwner._registrationLock)
            {
                foreach (var descriptor in descriptors)
                {
                    RegisterServiceDescriptor(descriptor, loaded);
                }
            }
            return loaded;
        }
        finally
        {
            RegistrationOwner._registrationGate.ExitWriteLock();
        }
    }

    private void RegisterServiceDescriptor(ServiceDescriptor serviceDescriptor, List<ServiceDescriptor> loadedServiceDescriptor)
    {
        // Note: We intentionally do NOT check for existing services here.
        // Microsoft's DI allows multiple registrations of the same service type,
        // which is used by libraries like gRPC, HttpClientFactory, and Options pattern
        // (e.g., multiple IConfigureOptions<T> registrations for named options).
        // See: https://github.com/RonSijm/RonSijm.Syringe/issues/1
        lock (RegistrationOwner._registrationLock)
        {
            Services.Add(serviceDescriptor);
            NewServices.Add(serviceDescriptor);
            loadedServiceDescriptor.Add(serviceDescriptor);
        }
    }

    private void BuildInitial()
    {
        var newServices = NewServices.ToList();
        PrepareDescriptors(newServices, true);
        RootProvider = Options.ServiceProviderBuilder == null ?
            Services.BuildServiceProvider(Options.ServiceProviderOptions) :
            Options.ServiceProviderBuilder(Services);

        DoAfterBuild(newServices, true);
        NewServices.Clear();
    }

    public void Build()
    {
        if (RegistrationOwner != this)
        {
            RegistrationOwner.Build();
            return;
        }

        _registrationGate.EnterWriteLock();
        try
        {
            lock (_registrationLock)
            {
                BuildInternal();
            }
        }
        finally
        {
            _registrationGate.ExitWriteLock();
        }
    }

    private void BuildInternal()
    {
        EnsureRegistrationHealthy();
        if (RootProvider == null)
        {
            BuildInitial();
            return;
        }

        var newServices = NewServices.ToList();
        var pendingServices = newServices.ToArray();
        var scopes = Scopes.Select(scope => scope.ServiceProvider.ScopedProvider).OfType<ServiceProviderEngineScope>();
        var checkpoint = new ServiceRegistrationCheckpoint(RootProvider, scopes, Options.AdditionalProviders);
        var committing = false;
        try
        {
            PrepareDescriptors(newServices, false);
            RootProvider.CallSiteFactory.AddDescriptors(newServices);
            InvalidateLookups(newServices);
            var commits = PrepareAfterBuild(newServices, false);
            foreach (var scope in Scopes.ToArray())
            {
                commits.AddRange(scope.ServiceProvider.PrepareAfterBuild(newServices, false));
            }
            committing = true;
            foreach (var commit in commits)
            {
                commit();
            }
            NewServices.Clear();
        }
        catch (Exception error)
        {
            if (committing)
            {
                // External commit callbacks cannot be undone through the DI/store APIs.
                _registrationFailure = error;
            }
            try
            {
                checkpoint.Restore();
            }
            catch (Exception cleanupError)
            {
                _registrationFailure = cleanupError;
                throw new AggregateException("Service registration failed and rollback cleanup also failed.", error, cleanupError);
            }
            finally
            {
                Services.Clear();
                foreach (var descriptor in RootProvider.CallSiteFactory.Descriptors)
                {
                    Services.Add(descriptor);
                }
                foreach (var descriptor in pendingServices)
                {
                    NewServices.Remove(descriptor);
                }
            }
            throw;
        }
    }

    private void InvalidateLookups(List<ServiceDescriptor> newServices)
    {
        var identifiers = newServices.Select(ServiceIdentifier.FromDescriptor).ToHashSet();
        foreach (var pair in RootProvider.ServiceAccessors.ToArray())
        {
            var type = pair.Key.ServiceType;
            var isEnumerable = type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
            if (isEnumerable || (identifiers.Contains(pair.Key) && pair.Value.CallSite == null))
            {
                RootProvider.ServiceAccessors.TryRemove(pair.Key, out _);
                if (isEnumerable)
                {
                    Options.AdditionalProviders.RemoveAll(provider => provider is SingletonProvider && provider.IsMatch(type));
                }
            }
        }
        foreach (var key in RootProvider.CallSiteFactory.CallSiteCache.Keys.ToArray())
        {
            var type = key.ServiceIdentifier.ServiceType;
            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                RootProvider.CallSiteFactory.CallSiteCache.TryRemove(key, out _);
            }
        }
    }

    private void EnsureRegistrationHealthy()
    {
        if (_registrationFailure != null)
        {
            throw new InvalidOperationException("A registration commit failed. This provider cannot be reused; dispose it and create a new provider.", _registrationFailure);
        }
    }

    private void PrepareDescriptors(List<ServiceDescriptor> services, bool initial)
    {
        var pending = services.ToArray();
        foreach (var extension in Options.AfterBuildExtensions.OfType<ISyringeBeforeBuildExtension>())
        {
            extension.SetReference(this);
            extension.PrepareDescriptors(services, initial);
        }
        foreach (var descriptor in pending)
        {
            var index = Services.ToList().FindLastIndex(candidate => ReferenceEquals(candidate, descriptor));
            if (index >= 0)
            {
                Services.RemoveAt(index);
            }
        }
        foreach (var descriptor in services)
        {
            Services.Add(descriptor);
        }
    }

    private List<Action> PrepareAfterBuild(List<ServiceDescriptor> services, bool initial)
    {
        var commits = new List<Action>();
        foreach (var extension in Options.AfterBuildExtensions)
        {
            extension.SetReference(this);
            try
            {
                if (extension is ISyringePreparedAfterBuildExtension prepared)
                {
                    commits.Add(prepared.Prepare(services, initial));
                }
                else
                {
                    extension.Process(services, initial);
                }
            }
            finally
            {
                extension.SetReference(RegistrationOwner);
            }
        }
        return commits;
    }

    private void DoAfterBuild(List<ServiceDescriptor> newServices, bool isInitialBuild)
    {
        foreach (var commit in PrepareAfterBuild(newServices, isInitialBuild))
        {
            commit();
        }
        // Note: We intentionally do NOT replace the scoped providers when rebuilding.
        // The existing scoped providers share the same RootProvider which has been updated
        // with the new service descriptors. Replacing the scoped providers would create
        // new instances of scoped services, breaking event subscriptions and other
        // references to the old instances (e.g., RadzenDialog subscribes to DialogService.OnOpen,
        // and replacing the scoped provider would give new components a different DialogService instance).
    }

    public void Dispose()
    {
        if (ScopedProvider is IDisposable scoped)
        {
            RegistrationOwner.Scopes.RemoveAll(scope => ReferenceEquals(scope.ServiceProvider, this));
            scoped.Dispose();
            return;
        }
        if (RootProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        Options.AdditionalProviders.Clear();
    }

    public ValueTask DisposeAsync()
    {
        if (ScopedProvider is IAsyncDisposable scoped)
        {
            RegistrationOwner.Scopes.RemoveAll(scope => ReferenceEquals(scope.ServiceProvider, this));
            return scoped.DisposeAsync();
        }
        if (RootProvider is IAsyncDisposable disposable)
        {
            return disposable.DisposeAsync();
        }

        return ValueTask.CompletedTask;
    }

    public virtual object GetKeyedService(Type serviceType, object serviceKey)
    {
        RegistrationOwner._registrationGate.EnterReadLock();
        try
        {
            RegistrationOwner.EnsureRegistrationHealthy();
            if (ScopedProvider is IKeyedServiceProvider scoped)
            {
                return scoped.GetKeyedService(serviceType, serviceKey);
            }
            return RootProvider.GetKeyedService(serviceType, serviceKey);
        }
        finally
        {
            RegistrationOwner._registrationGate.ExitReadLock();
        }
    }

    public virtual object GetRequiredKeyedService(Type serviceType, object serviceKey)
    {
        RegistrationOwner._registrationGate.EnterReadLock();
        try
        {
            RegistrationOwner.EnsureRegistrationHealthy();
            if (ScopedProvider is IKeyedServiceProvider scoped)
            {
                return scoped.GetRequiredKeyedService(serviceType, serviceKey);
            }
            return RootProvider.GetRequiredKeyedService(serviceType, serviceKey);
        }
        finally
        {
            RegistrationOwner._registrationGate.ExitReadLock();
        }
    }

    public void DisposeScope(ScopeWrapper scope)
    {
        RegistrationOwner.Scopes.Remove(scope);
        var disposable = scope.ServiceProvider.ScopedProvider as IDisposable;
        disposable?.Dispose();
    }
}