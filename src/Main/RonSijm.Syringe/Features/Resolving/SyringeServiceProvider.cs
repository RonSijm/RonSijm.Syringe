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
        var service = GetServiceWithoutExtensions(serviceType);
        Options.AfterGetServiceExtensions.ForEach(x => x.Decorate(service));

        return service;
    }

    public object GetServiceWithoutExtensions(Type serviceType)
    {
        try
        {
            var service = _innerProvider.GetService(serviceType);
            return service;
        }
        catch (Exception e)
        
        {
            Console.WriteLine("Exception: " + e.ToString());
            // TODO: Fallback
            return null;
        }
    }

    public async Task LoadServiceDescriptors(IAsyncEnumerable<ServiceDescriptor> serviceDescriptors)
    {
        await foreach (var serviceDescriptor in serviceDescriptors)
        {
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