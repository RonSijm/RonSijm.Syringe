// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

/// <summary>
/// Extension methods for building a <see cref="MicrosoftServiceProvider"/> from an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionContainerBuilderExtensions
{
    /// <summary>
    /// Creates a <see cref="MicrosoftServiceProvider"/> containing services from the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> containing service descriptors.</param>
    /// <returns>The <see cref="MicrosoftServiceProvider"/>.</returns>
    internal static MicrosoftServiceProvider BuildServiceProvider(this IServiceCollection services)
    {
        return BuildServiceProvider(services, ServiceProviderOptions.Default);
    }

    /// <summary>
    /// Creates a <see cref="MicrosoftServiceProvider"/> containing services from the provided <see cref="IServiceCollection"/>
    /// optionally enabling scope validation.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> containing service descriptors.</param>
    /// <param name="validateScopes">
    /// <c>true</c> to perform check verifying that scoped services never gets resolved from root provider; otherwise <c>false</c>.
    /// </param>
    /// <returns>The <see cref="MicrosoftServiceProvider"/>.</returns>
    internal static MicrosoftServiceProvider BuildServiceProvider(this IServiceCollection services, bool validateScopes)
    {
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = validateScopes });
    }

    /// <summary>
    /// Creates a <see cref="MicrosoftServiceProvider"/> containing services from the provided <see cref="IServiceCollection"/>
    /// optionally enabling scope validation.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> containing service descriptors.</param>
    /// <param name="options">
    /// Configures various service provider behaviors.
    /// </param>
    /// <returns>The <see cref="MicrosoftServiceProvider"/>.</returns>
    internal static MicrosoftServiceProvider BuildServiceProvider(this IServiceCollection services, ServiceProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        return new MicrosoftServiceProvider(services, options);
    }
}