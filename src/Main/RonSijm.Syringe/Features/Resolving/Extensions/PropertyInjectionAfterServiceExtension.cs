using System.Reflection;

namespace RonSijm.Syringe;

public class PropertyInjectionAfterServiceExtension : SyringeServiceProviderAfterServiceExtensionBase, IProvideCallSiteValidator
{
    public override void Decorate(Type serviceType, object service)
    {
        DecorateInternal(service, new List<AdditionProvider>());
    }

    public ISyringeCallSiteValidator CreateValidator(MicrosoftServiceProvider serviceProvider)
    {
        return new PropertyInjectionCallSiteValidator(serviceProvider);
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
                .Select(propertyInfo => (PropertyInfo: propertyInfo, InjectAttribute: propertyInfo.GetCustomAttributes().FirstOrDefault(attr => attr.GetType().Name == "InjectAttribute")))
                .Where(x => x.InjectAttribute != null)
                .ToList();

            foreach (var (propertyInfo, injectAttribute) in properties)
            {
                var key = GetKey(injectAttribute);
                var result = key != null
                    ? GetKeyedService(propertyInfo, key)
                    : GetService(propertyInfo, cacheProviders);

                if (result.Result == null && IsRequired(injectAttribute))
                {
                    throw new InvalidOperationException(BuildMissingMessage(propertyInfo, currentService.GetType(), key));
                }

                propertyInfo.SetValue(currentService, result.Result);

                if (result is { WireInner: true, Result: not null })
                {
                    queue.Enqueue(result.Result);
                }
            }
        }
    }

    private static string BuildMissingMessage(PropertyInfo propertyInfo, Type declaringType, object key)
    {
        return key != null
            ? $"Unable to resolve required property '{propertyInfo.Name}' of type '{propertyInfo.PropertyType}' with key '{key}' on '{declaringType.FullName}'."
            : $"Unable to resolve required property '{propertyInfo.Name}' of type '{propertyInfo.PropertyType}' on '{declaringType.FullName}'.";
    }

    private (object Result, bool WireInner) GetKeyedService(PropertyInfo propertyInfo, object key)
    {
        var serviceType = propertyInfo.PropertyType;
        var afterServiceExtensions = ServiceProvider.Options.AfterGetServiceExtensions
            .Where(x => x.GetType() != typeof(PropertyInjectionAfterServiceExtension))
            .ToList();

        var service = ServiceProvider.GetKeyedService(serviceType, key);
        if (service == null)
        {
            return (null, false);
        }

        afterServiceExtensions.ForEach(x => x.Decorate(serviceType, service));
        return (service, true);
    }

    private static bool IsRequired(Attribute injectAttribute)
    {
        var requiredProperty = injectAttribute.GetType().GetProperty("Required", BindingFlags.Public | BindingFlags.Instance);
        if (requiredProperty != null && requiredProperty.PropertyType == typeof(bool) && requiredProperty.CanRead)
        {
            return (bool)requiredProperty.GetValue(injectAttribute);
        }

        return false;
    }

    private static object GetKey(Attribute injectAttribute)
    {
        var keyProperty = injectAttribute.GetType().GetProperty("Key", BindingFlags.Public | BindingFlags.Instance);
        if (keyProperty == null || !keyProperty.CanRead)
        {
            return null;
        }

        var value = keyProperty.GetValue(injectAttribute);
        if (value is string stringValue)
        {
            return string.IsNullOrEmpty(stringValue) ? null : stringValue;
        }

        return value;
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
