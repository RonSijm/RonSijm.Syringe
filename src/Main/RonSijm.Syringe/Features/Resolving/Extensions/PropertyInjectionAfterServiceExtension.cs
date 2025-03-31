using System.Reflection;

namespace RonSijm.Syringe;

public class PropertyInjectionAfterServiceExtension : SyringeServiceProviderAfterServiceExtensionBase
{
    public override void Decorate(Type serviceType, object service)
    {
        DecorateInternal(service, new List<AdditionProvider>());
    }

    private void DecorateInternal(object service, List<AdditionProvider> cacheProviders)
    {
        if (service == null)
        {
            return;
        }

        var queue = new Queue<object>();
        queue.Enqueue(service);

        while (queue.Count > 0)
        {
            var currentService = queue.Dequeue();
            var properties = currentService.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.PropertyType != typeof(string))
                .Where(propertyInfo => propertyInfo.GetCustomAttributes().Any(attr => attr.GetType().Name == "InjectAttribute"))
                .ToList();

            foreach (var propertyInfo in properties)
            {
                var result = GetService(propertyInfo, cacheProviders);
                propertyInfo.SetValue(currentService, result.Result);

                if (result is { WireInner: true, Result: not null })
                {
                    queue.Enqueue(result.Result);
                }
            }
        }
    }

    private (object Result, bool WireInner) GetService(PropertyInfo propertyInfo, List<AdditionProvider> cacheProviders)
    {
        var serviceType = propertyInfo.PropertyType;
        var afterServiceExtensions = ServiceProvider.Options.AfterGetServiceExtensions
            .Where(x => x.GetType() != typeof(PropertyInjectionAfterServiceExtension))
            .ToList();

        if (ServiceProvider.TryGetServiceFromOverride(cacheProviders, serviceType, out var cacheValue))
        {
            afterServiceExtensions.ForEach(x => x.Decorate(serviceType, cacheValue));
            return (cacheValue, false);
        }

        if (ServiceProvider.TryGetServiceFromOverride(serviceType, out var value))
        {
            afterServiceExtensions.ForEach(x => x.Decorate(serviceType, value));
            return (value, true);
        }

        var service = ServiceProvider.GetServiceWithoutExtensions(serviceType);

        if (service == null)
        {
            return (value, false);
        }

        cacheProviders.Add(new SingletonProvider(serviceType, service));

        afterServiceExtensions.ForEach(x => x.Decorate(serviceType, service));

        ServiceProvider.TryAddDescriptorToCache(serviceType, service);

        return (service, true);
    }
}
