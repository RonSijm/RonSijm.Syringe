using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public class SyringeServiceProvider : IKeyedServiceProvider, IDisposable, IAsyncDisposable
{
    private readonly IKeyedServiceProvider _innerKeyedProvider;
    private readonly IServiceProvider _innerProvider;

    public SyringeServiceProvider(IServiceProvider innerProvider, params ISyringeServiceProviderAfterServiceExtension[] afterServiceExtensions)
    {
        _innerProvider = innerProvider;
        _innerKeyedProvider = innerProvider as IKeyedServiceProvider;
        _afterGetService = afterServiceExtensions?.ToList();
    }

    private readonly List<ISyringeServiceProviderAfterServiceExtension> _afterGetService;

    public SyringeServiceProvider(IServiceCollection descriptors, ServiceProviderOptions options, params ISyringeServiceProviderAfterServiceExtension[] afterServiceExtensions) : this(new MicrosoftServiceProvider(descriptors, options), afterServiceExtensions)
    {
    }

    public object GetService(Type serviceType)
    {
        var service = GetServiceWithoutExtensions(serviceType);

        _afterGetService.ForEach(x => x.Decorate(service));

        return service;
    }

    public object GetServiceWithoutExtensions(Type serviceType)
    {
        var service = _innerProvider.GetService(serviceType);
        return service;
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