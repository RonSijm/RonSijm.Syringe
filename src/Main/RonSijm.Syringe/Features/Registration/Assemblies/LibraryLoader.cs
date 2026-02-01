using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public static class LibraryLoader
{
    public static async IAsyncEnumerable<ServiceDescriptor> GetServiceDescriptorsOfAssemblies(Assembly assembly, SyringeServiceProviderOptions options = null, IServiceProvider serviceProvider = null)
    {
        await foreach (var registerAssembly in GetServiceDescriptorsOfAssemblies(options, serviceProvider, assembly))
        {
            yield return registerAssembly;
        }
    }

    public static IAsyncEnumerable<ServiceDescriptor> GetServiceDescriptorsOfAssemblies(SyringeServiceProviderOptions options, params Assembly[] assemblies)
    {
        var allAssemblyOptions = options?.GetOptions<AssemblyLoadOptions>();
        return GetServiceDescriptorsOfAssemblies(allAssemblyOptions, null, assemblies);
    }

    public static IAsyncEnumerable<ServiceDescriptor> GetServiceDescriptorsOfAssemblies(SyringeServiceProviderOptions options, IServiceProvider serviceProvider, params Assembly[] assemblies)
    {
        var allAssemblyOptions = options?.GetOptions<AssemblyLoadOptions>();
        return GetServiceDescriptorsOfAssemblies(allAssemblyOptions, serviceProvider, assemblies);
    }

    public static async IAsyncEnumerable<ServiceDescriptor> GetServiceDescriptorsOfAssemblies(AssemblyLoadOptions options, IServiceProvider serviceProvider, params Assembly[] assemblies)
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

            var registration = TryCreateInstance(assembly, expectedClassName, serviceProvider);

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

    private static object TryCreateInstance(Assembly assembly, string expectedClassName, IServiceProvider serviceProvider)
    {
        var type = assembly.GetType(expectedClassName);

        if (type == null)
        {
            return null;
        }

        // If we have a service provider, try to create instance with constructor injection
        if (serviceProvider != null)
        {
            try
            {
                // First, try to resolve constructor parameters from keyed services
                var constructorArgs = TryResolveConstructorArgs(type, serviceProvider);
                if (constructorArgs != null)
                {
                    return ActivatorUtilities.CreateInstance(serviceProvider, type, constructorArgs);
                }

                // Fall back to standard ActivatorUtilities
                return ActivatorUtilities.CreateInstance(serviceProvider, type);
            }
            catch
            {
                // Fall back to parameterless constructor if ActivatorUtilities fails
            }
        }

        // Fall back to parameterless constructor
        return assembly.CreateInstance(expectedClassName);
    }

    private static object[] TryResolveConstructorArgs(Type type, IServiceProvider serviceProvider)
    {
        // Get the keyed service provider if available
        if (serviceProvider is not IKeyedServiceProvider keyedProvider)
        {
            return null;
        }

        // Find the constructor with the most parameters (or the only one)
        var constructors = type.GetConstructors();
        if (constructors.Length == 0)
        {
            return null;
        }

        // Prefer constructor with parameters
        var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();
        var parameters = constructor.GetParameters();

        if (parameters.Length == 0)
        {
            return null;
        }

        var args = new object[parameters.Length];
        var allResolved = true;

        for (var i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];

            // Try to resolve as a keyed service using the parameter name as the key
            try
            {
                var keyedService = keyedProvider.GetKeyedService(param.ParameterType, param.Name);
                if (keyedService != null)
                {
                    args[i] = keyedService;
                    continue;
                }
            }
            catch
            {
                // Keyed service not found, continue
            }

            // Try to resolve as a regular service
            var service = serviceProvider.GetService(param.ParameterType);
            if (service != null)
            {
                args[i] = service;
                continue;
            }

            // If parameter has a default value, use it
            if (param.HasDefaultValue)
            {
                args[i] = param.DefaultValue;
                continue;
            }

            // Could not resolve this parameter
            allResolved = false;
            break;
        }

        return allResolved ? args : null;
    }
}