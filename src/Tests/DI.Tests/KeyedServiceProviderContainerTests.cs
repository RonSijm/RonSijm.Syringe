// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;
using RonSijm.Syringe;

namespace MicrosoftCopy.DependencyInjection.Tests;

public class KeyedServiceProviderDefaultContainerTests : KeyedDependencyInjectionSpecificationTests
{
    protected override IServiceProvider CreateServiceProvider(IServiceCollection collection) => collection.BuildServiceProvider(ServiceProviderMode.Default);
}

public class KeyedServiceProviderDynamicContainerTests : KeyedDependencyInjectionSpecificationTests
{
    protected override IServiceProvider CreateServiceProvider(IServiceCollection collection) => collection.BuildServiceProvider();
}

public class KeyedServiceProviderExpressionContainerTests : KeyedDependencyInjectionSpecificationTests
{
    protected override IServiceProvider CreateServiceProvider(IServiceCollection collection) => collection.BuildServiceProvider(ServiceProviderMode.Expressions);
}

public class KeyedServiceProviderILEmitContainerTests : KeyedDependencyInjectionSpecificationTests
{
    protected override IServiceProvider CreateServiceProvider(IServiceCollection collection) => collection.BuildServiceProvider(ServiceProviderMode.ILEmit);
}