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

            if (!serviceProvider.CallSiteFactory.IsService(propertyInfo.PropertyType))
            {
                throw new InvalidOperationException($"Unable to resolve required property '{propertyInfo.Name}' of type '{propertyInfo.PropertyType}' on '{implementationType.FullName}'.");
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
}
