// ReSharper disable UnusedMember.Global

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

/// <summary>
/// Extension methods for adding services to an <see cref="IHaveServiceCollection" />.
/// </summary>
public static class ServiceCollectionServiceExtensions
{
    /// <summary>
    /// Adds a transient service of the type specified in <paramref name="serviceType"/> with an
    /// implementation of the type specified in <paramref name="implementationType"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <param name="serviceType">The type of the service to register.</param>
    /// <param name="implementationType">The implementation type of the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Transient"/>
    public static T AddTransient<T>(this T services, Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType) where T : IHaveServiceCollection
    {
        services.InnerServiceCollection.AddTransient(serviceType, implementationType);
        return services;
    }

    /// <summary>
    /// Adds a transient service of the type specified in <paramref name="serviceType"/> with a
    /// factory specified in <paramref name="implementationFactory"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <param name="serviceType">The type of the service to register.</param>
    /// <param name="implementationFactory">The factory that creates the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Transient"/>
    public static T AddTransient<T>(this T services, Type serviceType, Func<IServiceProvider, object> implementationFactory) where T : IHaveServiceCollection
    {
        services.InnerServiceCollection.AddTransient(serviceType, implementationFactory);
        return services;
    }

    /// <summary>
    /// Adds a transient service of the type specified in <typeparamref name="TService"/> with an
    /// implementation type specified in <typeparamref name="TImplementation"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Transient"/>
    public static T AddTransient<T, TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this T services)
        where T : IHaveServiceCollection
        where TService : class
        where TImplementation : class, TService
    {
        services.InnerServiceCollection.AddTransient<TService, TImplementation>();
        return services;
    }

    /// <summary>
    /// Adds a transient service of the type specified in <paramref name="serviceType"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <param name="serviceType">The type of the service to register and the implementation to use.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Transient"/>
    public static T AddTransient<T>(this T services, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type serviceType) where T : IHaveServiceCollection
    {
        services.InnerServiceCollection.AddTransient(serviceType);
        return services;
    }

    /// <summary>
    /// Adds a transient service of the type specified in <typeparamref name="TService"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Transient"/>
    public static T AddTransient<T, TService>(this T services) where T : IHaveServiceCollection where TService : class
    {
        services.InnerServiceCollection.AddTransient<TService>();
        return services;
    }

    /// <summary>
    /// Adds a transient service of the type specified in <typeparamref name="TService"/> with a
    /// factory specified in <paramref name="implementationFactory"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <param name="implementationFactory">The factory that creates the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Transient"/>
    public static T AddTransient<T, TService>(this T services, Func<IServiceProvider, TService> implementationFactory) where T : IHaveServiceCollection where TService : class
    {
        services.InnerServiceCollection.AddTransient(implementationFactory);
        return services;
    }

    /// <summary>
    /// Adds a transient service of the type specified in <typeparamref name="TService"/> with an
    /// implementation type specified in <typeparamref name="TImplementation" /> using the
    /// factory specified in <paramref name="implementationFactory"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <param name="implementationFactory">The factory that creates the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Transient"/>
    public static T AddTransient<T, TService, TImplementation>(this T services, Func<IServiceProvider, TImplementation> implementationFactory)
        where T : IHaveServiceCollection
        where TService : class
        where TImplementation : class, TService
    {
        services.InnerServiceCollection.AddTransient<TService, TImplementation>(implementationFactory);
        return services;
    }

    /// <summary>
    /// Adds a scoped service of the type specified in <paramref name="serviceType"/> with an
    /// implementation of the type specified in <paramref name="implementationType"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <param name="serviceType">The type of the service to register.</param>
    /// <param name="implementationType">The implementation type of the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Scoped"/>
    public static T AddScoped<T>(this T services, Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType) where T : IHaveServiceCollection
    {
        services.InnerServiceCollection.AddScoped(serviceType, implementationType);
        return services;
    }

    /// <summary>
    /// Adds a scoped service of the type specified in <paramref name="serviceType"/> with a
    /// factory specified in <paramref name="implementationFactory"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <param name="serviceType">The type of the service to register.</param>
    /// <param name="implementationFactory">The factory that creates the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Scoped"/>
    public static T AddScoped<T>(this T services, Type serviceType, Func<IServiceProvider, object> implementationFactory) where T : IHaveServiceCollection
    {
        services.InnerServiceCollection.AddScoped(serviceType, implementationFactory);
        return services;
    }

    /// <summary>
    /// Adds a scoped service of the type specified in <typeparamref name="TService"/> with an
    /// implementation type specified in <typeparamref name="TImplementation"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Scoped"/>
    public static T AddScoped<T, TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this T services)
        where T : IHaveServiceCollection
        where TService : class
        where TImplementation : class, TService
    {
        services.InnerServiceCollection.AddScoped<TService, TImplementation>();
        return services;
    }

    /// <summary>
    /// Adds a scoped service of the type specified in <paramref name="serviceType"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <param name="serviceType">The type of the service to register and the implementation to use.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Scoped"/>
    public static T AddScoped<T>(this T services, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type serviceType) where T : IHaveServiceCollection
    {
        services.InnerServiceCollection.AddScoped(serviceType);
        return services;
    }

    /// <summary>
    /// Adds a scoped service of the type specified in <typeparamref name="TService"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Scoped"/>
    public static T AddScoped<T, TService>(this T services) where T : IHaveServiceCollection where TService : class
    {
        services.InnerServiceCollection.AddScoped<TService>();
        return services;
    }

    /// <summary>
    /// Adds a scoped service of the type specified in <typeparamref name="TService"/> with a
    /// factory specified in <paramref name="implementationFactory"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <param name="implementationFactory">The factory that creates the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Scoped"/>
    public static T AddScoped<T, TService>(this T services, Func<IServiceProvider, TService> implementationFactory) where T : IHaveServiceCollection where TService : class
    {
        services.InnerServiceCollection.AddScoped(implementationFactory);
        return services;
    }

    /// <summary>
    /// Adds a scoped service of the type specified in <typeparamref name="TService"/> with an
    /// implementation type specified in <typeparamref name="TImplementation" /> using the
    /// factory specified in <paramref name="implementationFactory"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <param name="implementationFactory">The factory that creates the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Scoped"/>
    public static T AddScoped<T, TService, TImplementation>(this T services, Func<IServiceProvider, TImplementation> implementationFactory)
        where T : IHaveServiceCollection
        where TService : class
        where TImplementation : class, TService
    {
        services.InnerServiceCollection.AddScoped<TService, TImplementation>(implementationFactory);
        return services;
    }

    /// <summary>
    /// Adds a singleton service of the type specified in <paramref name="serviceType"/> with an
    /// implementation of the type specified in <paramref name="implementationType"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <param name="serviceType">The type of the service to register.</param>
    /// <param name="implementationType">The implementation type of the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Singleton"/>
    public static T AddSingleton<T>(this T services, Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType) where T : IHaveServiceCollection
    {
        services.InnerServiceCollection.AddSingleton(serviceType, implementationType);
        return services;
    }

    /// <summary>
    /// Adds a singleton service of the type specified in <paramref name="serviceType"/> with a
    /// factory specified in <paramref name="implementationFactory"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <param name="serviceType">The type of the service to register.</param>
    /// <param name="implementationFactory">The factory that creates the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Singleton"/>
    public static T AddSingleton<T>(this T services, Type serviceType, Func<IServiceProvider, object> implementationFactory) where T : IHaveServiceCollection
    {
        services.InnerServiceCollection.AddSingleton(serviceType, implementationFactory);
        return services;
    }

    /// <summary>
    /// Adds a singleton service of the type specified in <typeparamref name="TService"/> with an
    /// implementation type specified in <typeparamref name="TImplementation"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Singleton"/>
    public static T AddSingleton<T, TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this T services)
        where T : IHaveServiceCollection
        where TService : class
        where TImplementation : class, TService
    {
        services.InnerServiceCollection.AddSingleton<TService, TImplementation>();
        return services;
    }

    /// <summary>
    /// Adds a singleton service of the type specified in <paramref name="serviceType"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <param name="serviceType">The type of the service to register and the implementation to use.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Singleton"/>
    public static T AddSingleton<T>(this T services, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type serviceType) where T : IHaveServiceCollection
    {
        services.InnerServiceCollection.AddSingleton(serviceType);
        return services;
    }

    /// <summary>
    /// Adds a singleton service of the type specified in <typeparamref name="TService"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Singleton"/>
    public static T AddSingleton<T, TService>(this T services) where T : IHaveServiceCollection where TService : class
    {
        services.InnerServiceCollection.AddSingleton<TService>();
        return services;
    }

    /// <summary>
    /// Adds a singleton service of the type specified in <typeparamref name="TService"/> with a
    /// factory specified in <paramref name="implementationFactory"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <param name="implementationFactory">The factory that creates the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Singleton"/>
    public static T AddSingleton<T, TService>(this T services, Func<IServiceProvider, TService> implementationFactory) where T : IHaveServiceCollection where TService : class
    {
        services.InnerServiceCollection.AddSingleton(implementationFactory);
        return services;
    }

    /// <summary>
    /// Adds a singleton service of the type specified in <typeparamref name="TService"/> with an
    /// implementation type specified in <typeparamref name="TImplementation" /> using the
    /// factory specified in <paramref name="implementationFactory"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <param name="implementationFactory">The factory that creates the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Singleton"/>
    public static T AddSingleton<T, TService, TImplementation>(this T services, Func<IServiceProvider, TImplementation> implementationFactory)
        where T : IHaveServiceCollection
        where TService : class
        where TImplementation : class, TService
    {
        services.InnerServiceCollection.AddSingleton<TService, TImplementation>(implementationFactory);
        return services;
    }

    /// <summary>
    /// Adds a singleton service of the type specified in <paramref name="serviceType"/> with an
    /// instance specified in <paramref name="implementationInstance"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <param name="serviceType">The type of the service to register.</param>
    /// <param name="implementationInstance">The instance of the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Singleton"/>
    public static T AddSingleton<T>(this T services, Type serviceType, object implementationInstance) where T : IHaveServiceCollection
    {
        services.InnerServiceCollection.AddSingleton(serviceType, implementationInstance);
        return services;
    }

    /// <summary>
    /// Adds a singleton service of the type specified in <typeparamref name="TService" /> with an
    /// instance specified in <paramref name="implementationInstance"/> to the
    /// specified <see cref="IHaveServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service collection that implements <see cref="IHaveServiceCollection"/>.</typeparam>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <param name="services">The <see cref="IHaveServiceCollection"/> to add the service to.</param>
    /// <param name="implementationInstance">The instance of the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="ServiceLifetime.Singleton"/>
    public static T AddSingleton<T, TService>(this T services, TService implementationInstance) where T : IHaveServiceCollection where TService : class
    {
        services.InnerServiceCollection.AddSingleton(implementationInstance);
        return services;
    }
}