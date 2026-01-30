using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

/// <summary>
/// Interface for extensions that are called after assemblies are loaded
/// </summary>
public interface ILoadAfterExtension
{
    void AssembliesLoaded(List<Assembly> loadedAssemblies);
    void DescriptorsLoaded(List<ServiceDescriptor> loadedDescriptors);
    void SetReference(SyringeServiceProvider serviceProvider);
}

/// <summary>
/// Base implementation of ILoadAfterExtension with default no-op implementations
/// </summary>
public abstract class BaseLoadAfterExtension : ILoadAfterExtension
{
    public SyringeServiceProvider ServiceProvider { get; protected set; }

    public virtual void AssembliesLoaded(List<Assembly> loadedAssemblies)
    {
        // Do nothing by default
    }

    public virtual void DescriptorsLoaded(List<ServiceDescriptor> loadedDescriptors)
    {
        // Do nothing by default
    }

    public virtual void SetReference(SyringeServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
}
