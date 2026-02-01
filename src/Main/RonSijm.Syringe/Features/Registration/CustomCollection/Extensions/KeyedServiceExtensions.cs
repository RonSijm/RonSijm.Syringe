using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.Interfaces;

namespace RonSijm.Syringe;

/// <summary>
/// Extension methods for adding keyed services (including value types) to an <see cref="IHaveServiceCollection" />.
/// </summary>
public static class KeyedServiceExtensions
{
    /// <summary>
    /// Adds a singleton service of the type specified in <typeparamref name="TService"/> with the specified key and instance
    /// to the specified <see cref="IHaveServiceCollection"/>. This overload supports value types.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <param name="services">The <see cref="SyringeServiceCollection"/> to add the service to.</param>
    /// <param name="serviceKey">The key of the service.</param>
    /// <param name="instance">The instance of the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static SyringeServiceCollection AddKeyedSingleton<TService>(this SyringeServiceCollection services, object serviceKey, TService instance)
    {
        services.InnerServiceCollection.AddKeyedSingleton(typeof(TService), serviceKey, instance);
        return services;
    }

    /// <summary>
    /// Adds a scoped service of the type specified in <typeparamref name="TService"/> with the specified key and factory
    /// to the specified <see cref="SyringeServiceCollection"/>. This overload supports value types.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <param name="services">The <see cref="SyringeServiceCollection"/> to add the service to.</param>
    /// <param name="serviceKey">The key of the service.</param>
    /// <param name="implementationFactory">The factory that creates the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static SyringeServiceCollection AddKeyedScoped<TService>(this SyringeServiceCollection services, object serviceKey, Func<IServiceProvider, object, TService> implementationFactory)
    {
        services.InnerServiceCollection.AddKeyedScoped(typeof(TService), serviceKey, (sp, key) => implementationFactory(sp, key));
        return services;
    }

    /// <summary>
    /// Adds a transient service of the type specified in <typeparamref name="TService"/> with the specified key and factory
    /// to the specified <see cref="SyringeServiceCollection"/>. This overload supports value types.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <param name="services">The <see cref="SyringeServiceCollection"/> to add the service to.</param>
    /// <param name="serviceKey">The key of the service.</param>
    /// <param name="implementationFactory">The factory that creates the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static SyringeServiceCollection AddKeyedTransient<TService>(this SyringeServiceCollection services, object serviceKey, Func<IServiceProvider, object, TService> implementationFactory)
    {
        services.InnerServiceCollection.AddKeyedTransient(typeof(TService), serviceKey, (sp, key) => implementationFactory(sp, key));
        return services;
    }
}

