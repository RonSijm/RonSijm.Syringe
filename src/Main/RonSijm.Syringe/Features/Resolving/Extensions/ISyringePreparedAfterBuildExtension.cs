using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public interface ISyringePreparedAfterBuildExtension : ISyringeAfterBuildExtension
{
    /// <summary>Validate without changing existing services, then return a non-throwing commit action.</summary>
    Action Prepare(List<ServiceDescriptor> services, bool isInitialBuild);
}
