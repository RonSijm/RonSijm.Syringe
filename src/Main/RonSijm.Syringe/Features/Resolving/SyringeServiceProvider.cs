using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.Scope;

namespace RonSijm.Syringe;

public class SyringeServiceProvider : IKeyedServiceProvider, IDisposable, IAsyncDisposable
{
    private IKeyedServiceProvider _innerKeyedProvider;
    private IServiceProvider _innerProvider;
    public SyringeServiceProviderOptions Options { get; private set; }
    internal IServiceCollection Services { get; private set; }
    
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

        if (Options.BuildOnConstruct)
        {
            Build();
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

        var descriptor = Services.FirstOrDefault(x => x.ServiceType == serviceType);

        if (descriptor is { Lifetime: ServiceLifetime.Singleton })
        {
            Options.AdditionalProviders.Add(new SingletonProvider(service));
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

    public async Task LoadServiceDescriptors(IAsyncEnumerable<ServiceDescriptor> serviceDescriptors)
    {
        await foreach (var serviceDescriptor in serviceDescriptors)
        {
            var existingService = Services.FirstOrDefault(x => x.ServiceType == serviceDescriptor.ServiceType);

            if (existingService != null)
            {
                continue;
            }

            Services.Add(serviceDescriptor);
        }
    }

    public void LoadServiceDescriptors(ServiceCollection serviceDescriptors)
    {
        foreach (var serviceDescriptor in serviceDescriptors)
        {
            Services.Add(serviceDescriptor);
        }
    }

    public void ReloadServiceDescriptors(ServiceCollection serviceDescriptors)
    {
        foreach (var serviceDescriptor in serviceDescriptors)
        {
            var existingService = Services.Where(x => x.ServiceType == serviceDescriptor.ServiceType).ToList();

            foreach (var descriptor in existingService)
            {
                Services.Remove(descriptor);
            }

            Services.Add(serviceDescriptor);
        }
    }

    public void Build()
    {
        _innerProvider = Options.ServiceProviderBuilder == null ?
            Services.BuildServiceProvider(Options.ServiceProviderOptions) :
            Options.ServiceProviderBuilder(Services);

        _innerKeyedProvider = _innerProvider as IKeyedServiceProvider;
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
        return _innerKeyedProvider.GetKeyedService(serviceType, serviceKey);
    }

    public object GetRequiredKeyedService(Type serviceType, object serviceKey)
    {
        return _innerKeyedProvider.GetRequiredKeyedService(serviceType, serviceKey);
    }
}