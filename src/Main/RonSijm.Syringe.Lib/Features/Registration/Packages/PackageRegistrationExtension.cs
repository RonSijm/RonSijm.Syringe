using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public static class PackageRegistrationExtension
{
    public static async Task<IServiceCollection> Register<T>(this IServiceCollection services) where T : IBootstrapper, new()
    {
        var package = new T();

        return await Register(services, package);
    }

    public static async Task<IServiceCollection> Register(this IServiceCollection services, IBootstrapper package)
    {
        foreach (var descriptor in await package.Bootstrap())
        {
            services.Add(descriptor);
        }

        return services;
    }
}