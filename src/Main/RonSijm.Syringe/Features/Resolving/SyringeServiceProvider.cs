using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.Scope;
using RonSijm.Syringe.ServiceLookup;

namespace RonSijm.Syringe;

public class SyringeServiceProvider : IKeyedServiceProvider, IDisposable, IAsyncDisposable
{
    private MicrosoftServiceProvider _innerProvider;
    public SyringeServiceProviderOptions Options { get; private set; }
    internal IServiceCollection Services { get; private set; }
    internal List<ServiceDescriptor> NewServices { get; private set; }
    
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
        Options.ServiceProviderOptions ??= new ServiceProviderOptions() { RegisterServiceScopeFactory = false };
        Services = collection;
        NewServices = [];
        
        foreach (var item in collection)
        {
            NewServices.Add(item);
        }

        collection.AddScoped(typeof(Optional<>), typeof(Optional<>));
        collection.AddSingleton<IServiceScopeFactory>(_ => new SyringeServiceScopeFactory(this));
        collection.AddSingleton<IServiceProvider>(this);
        collection.AddSingleton(this);

        if (Options.Services != null)
        {
            var collectionFromOptions = Options.Services.BuildServiceCollection();

            foreach (var collectionFromOption in collectionFromOptions)
            {
                collection.Add(collectionFromOption);
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
            Build(true);
        }
    }

    public object GetService(Type serviceType)
    {
        if (TryGetServiceFromOverride(serviceType, out var value))
        {
            Options.AfterGetServiceExtensions.ForEach(x => x.Decorate(value));
            return value;
        }

        var service = GetServiceWithoutExtensions(serviceType);
        Options.AfterGetServiceExtensions.ForEach(x => x.Decorate(service));

        if (service == null)
        {
            return null;
        }

        var descriptor = _innerProvider.CallSiteFactory.CallSiteCache.FirstOrDefault(x => x.Key.ServiceIdentifier.ServiceType == serviceType);

        if (descriptor.Value?.Cache is { Location: CallSiteResultCacheLocation.Root })
        {
            Options.AdditionalProviders.Add(new SingletonProvider(serviceType, service));
        }

        return service;
    }

    public object GetServiceWithoutExtensions(Type serviceType)
    {
        var service = _innerProvider.GetService(serviceType);
        return service;
    }

    public bool TryGetServiceFromOverride(Type serviceType, out object value)
    {
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator - Justification: Generates shitty linq
        foreach (var typeFunctionOverride in Options.AdditionalProviders)
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

    public async Task<List<ServiceDescriptor>> LoadServiceDescriptors(IAsyncEnumerable<ServiceDescriptor> serviceDescriptors)
    {
        var loadedServiceDescriptor = new List<ServiceDescriptor>();

        await foreach (var serviceDescriptor in serviceDescriptors)
        {
            var existingService = Services.FirstOrDefault(x => x.ServiceType == serviceDescriptor.ServiceType);

            if (existingService != null)
            {
                continue;
            }

            Services.Add(serviceDescriptor);
            NewServices.Add(serviceDescriptor);
            loadedServiceDescriptor.Add(serviceDescriptor);
        }

        return loadedServiceDescriptor;
    }

    public void Build(bool isInitialBuild)
    {
        _innerProvider = Options.ServiceProviderBuilder == null ?
            Services.BuildServiceProvider(Options.ServiceProviderOptions) :
            Options.ServiceProviderBuilder(Services);

        var newServices = NewServices.ToList();
        NewServices.Clear();

        Options.AfterBuildExtensions.ForEach(x => x.Process(newServices, isInitialBuild));
    }

    public void Dispose()
    {
        if (_innerProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public ValueTask DisposeAsync()
    {
        if (_innerProvider is IAsyncDisposable disposable)
        {
            return disposable.DisposeAsync();
        }

        return ValueTask.CompletedTask;
    }

    public object GetKeyedService(Type serviceType, object serviceKey)
    {
        return _innerProvider.GetKeyedService(serviceType, serviceKey);
    }

    public object GetRequiredKeyedService(Type serviceType, object serviceKey)
    {
        return _innerProvider.GetRequiredKeyedService(serviceType, serviceKey);
    }
}