using System.Reflection;

namespace RonSijm.Syringe;
public static class BootstrapExtensions
{
    public static async Task RegisterAssemblies(this SyringeServiceProvider provider, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var serviceDescriptors = LibraryLoader.GetServiceDescriptorsOfAssemblies(provider.Options, assemblies);
            var result = await provider.LoadServiceDescriptors(serviceDescriptors);
        }
    }

    public static async Task RegisterAssembly(this SyringeServiceProvider provider, Assembly assembly)
    {
        var serviceDescriptors = LibraryLoader.GetServiceDescriptorsOfAssemblies(provider.Options, assembly);
        var result = await provider.LoadServiceDescriptors(serviceDescriptors);
    }
}
