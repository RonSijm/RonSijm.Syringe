using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.Interfaces;

namespace RonSijm.Syringe.Helpers;

public static class AddFuncExtensions
{
    public static SyringeServiceCollectionAndRegistration AddScopedFunc<TService>(this SyringeServiceCollectionAndRegistration services)
    {
        services.InnerServiceCollection.AddScopedFunc<TService>();
        return services;
    }

    public static SyringeServiceCollection AddScopedFunc<TService>(this SyringeServiceCollection services)
    {
        services.InnerServiceCollection.AddScopedFunc<TService>();
        return services;
    }

    public static T AddScopedFunc<T, TService>(this T services) where T : IHaveServiceCollection
    {
        services.InnerServiceCollection.AddScopedFunc<TService>();
        return services;
    }

    public static IServiceCollection AddScopedFunc<T>(this IServiceCollection services)
    {
        services.AddFunc<T>(ServiceLifetime.Scoped);
        return services;
    }

    public static IServiceCollection AddFunc<T>(this IServiceCollection services, ServiceLifetime lifetime)
    {
        services.Add(new ServiceDescriptor(typeof(Func<T>), factory => new Func<T>(factory.GetRequiredService<T>), lifetime));
        return services;
    }
}