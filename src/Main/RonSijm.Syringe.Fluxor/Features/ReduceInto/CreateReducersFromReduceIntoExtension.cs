using System.Reflection;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public class CreateReducersFromReduceIntoExtension : ISyringeAfterBuildExtension
{
    private SyringeServiceProvider _serviceProvider;

    public void SetReference(SyringeServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Process(List<ServiceDescriptor> loadedDescriptors, bool isInitialBuild)
    {
        if (loadedDescriptors == null || !loadedDescriptors.Any())
        {
            return;
        }

        var store = _serviceProvider.GetService<IStore>();
        var featureDescriptors = loadedDescriptors.Where(x => x.ServiceType.IsAssignableTo(typeof(IFeature))).ToList();

        var processedItems = new List<object>();

        foreach (var serviceDescriptor in featureDescriptors)
        {
            var service = _serviceProvider.GetService(serviceDescriptor.ServiceType) as IFeature;

            if (processedItems.Contains(service))
            {
                continue;
            }

            processedItems.Add(service);
            var stateType = service.GetStateType();

            var properties = stateType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.IsDefined(typeof(ReduceIntoAttribute), inherit: true))
                {
                    propertyInfo.HandleReduceIntoAttribute(stateType, service);
                }
                else if (propertyInfo.IsDefined(typeof(ReduceFromAttribute), inherit: true))
                {
                    propertyInfo.HandleReduceFromAttribute(stateType, _serviceProvider);
                }
            }
        }
    }
}