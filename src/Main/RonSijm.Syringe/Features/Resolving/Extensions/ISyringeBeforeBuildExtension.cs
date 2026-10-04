using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public interface ISyringeBeforeBuildExtension : ISyringeExtension
{
    /// <summary>Validate and normalize pending descriptors before they join the provider.</summary>
    void PrepareDescriptors(List<ServiceDescriptor> services, bool isInitialBuild);
}
