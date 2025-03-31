using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public interface ISyringeAfterBuildExtension : ISyringeExtension
{
    void Process(List<ServiceDescriptor> service, bool isInitialBuild);
}