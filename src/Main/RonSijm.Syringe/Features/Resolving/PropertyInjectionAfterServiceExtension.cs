using System.Reflection;

namespace RonSijm.Syringe;

public class PropertyInjectionAfterServiceExtension : SyringeServiceProviderAfterServiceExtensionBase
{
    public override void Decorate(object service)
    {
        if (service == null)
        {
            return;
        }

        var properties = service.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && p.PropertyType != typeof(string) && ServiceProvider.GetServiceWithoutExtensions(p.PropertyType) != null);

        foreach (var property in properties)
        {
            property.SetValue(service, ServiceProvider.GetServiceWithoutExtensions(property.PropertyType));
        }
    }
}