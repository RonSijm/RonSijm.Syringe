using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public interface ISyringeExtension
{
    void SetReference(SyringeServiceProvider serviceProvider);
}

public interface ISyringeServiceProviderAfterServiceExtension : ISyringeExtension
{
    void Decorate(object service);
}

public interface ISyringeAfterBuildExtension : ISyringeExtension
{
    void Process(List<ServiceDescriptor> service, bool isInitialBuild);
}