using System.Reflection;
using System.Runtime.CompilerServices;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public class CreateReducersFromReduceIntoExtension : ISyringePreparedAfterBuildExtension
{
    private SyringeServiceProvider _serviceProvider;
    private readonly ConditionalWeakTable<IFeature, HashSet<PropertyInfo>> _properties = new();
    private readonly ConditionalWeakTable<List<ServiceDescriptor>, HashSet<IFeature>> _preparedFeatures = new();

    public void SetReference(SyringeServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Process(List<ServiceDescriptor> descriptors, bool isInitialBuild)
    {
        Prepare(descriptors, isInitialBuild)();
    }

    public Action Prepare(List<ServiceDescriptor> descriptors, bool isInitialBuild)
    {
        var provider = _serviceProvider;
        if (provider.IsRootScope && provider.ServiceDescriptors.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IStore))?.Lifetime == ServiceLifetime.Scoped)
        {
            return () => { };
        }

        var features = descriptors.Where(descriptor => !descriptor.IsKeyedService && typeof(IFeature).IsAssignableFrom(descriptor.ServiceType))
            .Select(descriptor => (IFeature)provider.GetRequiredService(descriptor.ServiceType)).Distinct();
        var commits = new List<Action>();
        foreach (var feature in features)
        {
            if (!_preparedFeatures.GetOrCreateValue(descriptors).Add(feature))
            {
                continue;
            }
            var attached = _properties.GetOrCreateValue(feature);
            var stateType = feature.GetStateType();
            foreach (var property in stateType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(property => !attached.Contains(property)))
            {
                Action commit;
                if (property.IsDefined(typeof(ReduceIntoAttribute), inherit: true))
                {
                    if (!property.CanWrite)
                    {
                        throw new InvalidOperationException($"ReduceInto property '{stateType.FullName}.{property.Name}' must be writable.");
                    }
                    commit = property.PrepareReduceIntoAttribute(stateType, feature);
                }
                else if (property.IsDefined(typeof(ReduceFromAttribute), inherit: true))
                {
                    commit = property.PrepareReduceFromAttribute(stateType, provider);
                }
                else
                {
                    continue;
                }
                commits.Add(() =>
                {
                    commit();
                    attached.Add(property);
                });
            }
        }
        return () => commits.ForEach(commit => commit());
    }
}
