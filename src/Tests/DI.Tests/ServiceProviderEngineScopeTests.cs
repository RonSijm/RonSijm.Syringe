// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using RonSijm.Syringe;
using RonSijm.Syringe.ServiceLookup;
using Xunit;

namespace MicrosoftCopy.DependencyInjection.Tests;

public class ServiceProviderEngineScopeTests
{
    [Fact]
    public void DoubleDisposeWorks()
    {
        var provider = new MicrosoftServiceProvider(new ServiceCollection(), ServiceProviderOptions.Default);
        var serviceProviderEngineScope = new ServiceProviderEngineScope(provider, isRootScope: true);
        serviceProviderEngineScope.ResolvedServices.Add(new ServiceCacheKey(ServiceIdentifier.FromServiceType(typeof(IFakeService)), 0), null);
        serviceProviderEngineScope.Dispose();
        serviceProviderEngineScope.Dispose();
    }

    [Fact]
    public void RootEngineScopeDisposeTest()
    {
        var services = new ServiceCollection();
        var sp = new SyringeServiceProvider(services.BuildServiceProvider());
        var s = sp.GetRequiredService<IServiceProvider>();
        ((IDisposable)s).Dispose();

        Assert.Throws<ObjectDisposedException>(() => sp.GetRequiredService<IServiceProvider>());
    }

    [Fact]
    public void ServiceProviderEngineScope_ImplementsAllServiceProviderInterfaces()
    {
        var engineScopeInterfaces = typeof(ServiceProviderEngineScope).GetInterfaces();
        foreach (var serviceProviderInterface in typeof(SyringeServiceProvider).GetInterfaces())
        {
            Assert.Contains(serviceProviderInterface, engineScopeInterfaces);
        }
    }
}