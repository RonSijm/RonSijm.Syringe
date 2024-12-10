using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

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
        services.AddScoped(typeof(Func<T>), provider => new Func<T>(provider.GetRequiredService<T>));
        return services;
    }
}