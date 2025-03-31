using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public static class LibraryLoader
{
    public static async IAsyncEnumerable<ServiceDescriptor> GetServiceDescriptorsOfAssemblies(Assembly assembly, SyringeServiceProviderOptions options = null)
    {
        await foreach (var registerAssembly in GetServiceDescriptorsOfAssemblies(options, assembly))
        {
            yield return registerAssembly;
        }
    }

    public static IAsyncEnumerable<ServiceDescriptor> GetServiceDescriptorsOfAssemblies(SyringeServiceProviderOptions options, params Assembly[] assemblies)
    {
        var allAssemblyOptions = options?.GetOptions<AssemblyLoadOptions>();
        return GetServiceDescriptorsOfAssemblies(allAssemblyOptions, assemblies);
    }

    public static async IAsyncEnumerable<ServiceDescriptor> GetServiceDescriptorsOfAssemblies(AssemblyLoadOptions options, params Assembly[] assemblies)
    {
        if (assemblies == null)
        {
            yield break;
        }

        foreach (var assembly in assemblies)
        {
            var assemblyName = $"{assembly.GetName().Name}";
            var assemblyOptions = options?.GetOptions(assemblyName);
            var expectedClassName = assemblyOptions?.ClassPath ?? $"{assemblyName}.Properties.BlazyBootstrap";
            var registration = assembly.CreateInstance(expectedClassName);

            if (registration == null)
            {
                continue;
            }

            var services = await BootstrapLoader.TryLoadServices(registration);

            // If we weren't able to get the services from the assembly, we continue
            if (services == null)
            {
                // If there is a class but no method, that's an error, so we log out it.
                // Don't think there's a need to throw anything.
                Console.Write(@"Error: BlazyBootstrap Class found but no GetServices");
                continue;
            }

            foreach (var serviceDescriptor in services)
            {
                yield return serviceDescriptor;
            }
        }
    }
}