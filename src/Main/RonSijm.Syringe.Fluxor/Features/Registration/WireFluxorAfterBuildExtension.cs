using Fluxor;
using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public class WireFluxorAfterBuildExtension : ISyringeAfterBuildExtension
{
    private SyringeServiceProvider _serviceProvider;

    public void SetReference(SyringeServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Process(List<ServiceDescriptor> loadedDescriptors, bool isInitialBuild)
    {
        if (isInitialBuild || loadedDescriptors == null || !loadedDescriptors.Any())
        {
            if (_serviceProvider.Options.AfterGetServiceExtensions.Count != 0)
            {
                // Since services are create through the activator, we'll resolve them and inject properties.
                // We don't have to do this if there are no extra AfterGetServiceExtensions
                HandleAfterServiceExtensions(loadedDescriptors);
            }

            return;
        }

        AddLoadedFluxorTypesToStore(loadedDescriptors);
    }

    private void AddLoadedFluxorTypesToStore(List<ServiceDescriptor> loadedDescriptors)
    {
        var otherTypes = new List<ServiceDescriptor>();
        var store = _serviceProvider.GetService<IStore>();

        foreach (var serviceDescriptor in loadedDescriptors)
        {
            if (serviceDescriptor.ServiceType.IsAssignableTo(typeof(IFeature)))
            {
                var service = _serviceProvider.GetService(serviceDescriptor.ServiceType) as IFeature;
                store.AddFeature(service);
            }
            else if (serviceDescriptor.ServiceType.IsAssignableTo(typeof(IEffect)))
            {
                var service = _serviceProvider.GetService(serviceDescriptor.ServiceType) as IEffect;
                store.AddEffect(service);
            }
            else if (serviceDescriptor.ServiceType.IsAssignableTo(typeof(IMiddleware)))
            {
                var service = _serviceProvider.GetService(serviceDescriptor.ServiceType) as IMiddleware;
                store.AddMiddleware(service);
            }
            else
            {
                otherTypes.Add(serviceDescriptor);
            }
        }
    }

    private void HandleAfterServiceExtensions(List<ServiceDescriptor> loadedDescriptors)
    {
        var otherTypes = new List<ServiceDescriptor>();
        var store = _serviceProvider.GetService<IStore>();

        var applicableTypes = loadedDescriptors.Where(x => x.ServiceType.IsAssignableTo(typeof(IFeature)) || x.ServiceType.IsAssignableTo(typeof(IEffect)) || x.ServiceType.IsAssignableTo(typeof(IMiddleware))).ToList();

        foreach (var serviceDescriptor in applicableTypes)
        {
            try
            {
                var service = _serviceProvider.GetService(serviceDescriptor.ServiceType);
            }
            catch (Exception)
            {

            }
        }
    }
}