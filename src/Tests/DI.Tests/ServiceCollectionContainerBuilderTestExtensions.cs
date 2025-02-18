// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe;
using RonSijm.Syringe.ServiceLookup;
using RonSijm.Syringe.ServiceLookup.Expressions;
using RonSijm.Syringe.ServiceLookup.ILEmit;

namespace MicrosoftCopy.DependencyInjection.Tests;

internal static class ServiceCollectionContainerBuilderTestExtensions
{
    public static SyringeServiceProvider BuildServiceProvider(this IServiceCollection services, ServiceProviderMode mode, ServiceProviderOptions options = null)
    {
        options ??= ServiceProviderOptions.Default;

        if (mode == ServiceProviderMode.Default)
        {
            return new SyringeServiceProvider(services, providerOptions =>
            {
                providerOptions.ServiceProviderBuilder = s => s.BuildServiceProvider(options);
            });
        }

        return new SyringeServiceProvider(services, providerOptions =>
        {
            providerOptions.ServiceProviderBuilder = serviceCollection =>
            {
                var provider = new MicrosoftServiceProvider(serviceCollection, ServiceProviderOptions.Default);
                ServiceProviderEngine engine = mode switch
                {
                    ServiceProviderMode.Dynamic => new DynamicServiceProviderEngine(provider),
                    ServiceProviderMode.Runtime => RuntimeServiceProviderEngine.Instance,
                    ServiceProviderMode.Expressions => new ExpressionsServiceProviderEngine(provider),
                    ServiceProviderMode.ILEmit => new ILEmitServiceProviderEngine(provider),
                    _ => throw new NotSupportedException()
                };
                provider._engine = engine;

                return provider;
            };
        });
    }
}