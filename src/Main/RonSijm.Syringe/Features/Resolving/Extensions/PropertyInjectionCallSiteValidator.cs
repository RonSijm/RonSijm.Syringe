using System.Reflection;
using RonSijm.Syringe.ServiceLookup;

namespace RonSijm.Syringe;

internal sealed class PropertyInjectionCallSiteValidator(MicrosoftServiceProvider serviceProvider) : ISyringeCallSiteValidator
{
    public void ValidateCallSite(ServiceCallSite callSite)
    {
        if (callSite is not ConstructorCallSite constructorCallSite)
        {
            return;
        }

        var implementationType = constructorCallSite.ImplementationType;
        if (implementationType == null)
        {
            return;
        }

        var properties = implementationType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && p.PropertyType != typeof(string));

        foreach (var propertyInfo in properties)
        {
            var injectAttribute = propertyInfo.GetCustomAttributes().FirstOrDefault(attr => attr.GetType().Name == "InjectAttribute");
            if (injectAttribute == null || !IsRequired(injectAttribute))
            {
                continue;
            }

            var key = GetKey(injectAttribute);
            var isRegistered = key != null
                ? serviceProvider.CallSiteFactory.IsKeyedService(propertyInfo.PropertyType, key)
                : serviceProvider.CallSiteFactory.IsService(propertyInfo.PropertyType);

            if (!isRegistered)
            {
                throw new InvalidOperationException(key != null
                    ? $"Unable to resolve required property '{propertyInfo.Name}' of type '{propertyInfo.PropertyType}' with key '{key}' on '{implementationType.FullName}'."
                    : $"Unable to resolve required property '{propertyInfo.Name}' of type '{propertyInfo.PropertyType}' on '{implementationType.FullName}'.");
            }
        }
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
}
