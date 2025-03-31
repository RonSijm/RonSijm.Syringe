using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public static class BootstrapLoader
{
    public static async Task<IEnumerable<ServiceDescriptor>> TryLoadServices(object registration)
    {
        // We can't throw an exception, we can't be certain that all lazy loaded assemblies require bootstrapping.
        IEnumerable<ServiceDescriptor> services = null;
        // If the registration is interfaced, it's easiest to use that.
        if (registration is IBootstrapper registrationAsInterface)
        {
            services = await registrationAsInterface.Bootstrap();
        }
        else
        {
            // Otherwise we try to GetServices through reflection.
            var registrationMethodInfo = registration.GetType().GetMethod(nameof(IBootstrapper.Bootstrap));
            if (registrationMethodInfo == null)
            {
                return null;
            }

            if (registrationMethodInfo.Invoke(registration, []) is Task<IEnumerable<ServiceDescriptor>> taskResult)
            {
                services = await taskResult;
            }
        }

        return services;
    }
}