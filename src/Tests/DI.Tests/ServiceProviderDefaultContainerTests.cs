// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;

namespace MicrosoftCopy.DependencyInjection.Tests;

public class ServiceProviderDefaultContainerTests : ServiceProviderContainerTests
{
    protected override IServiceProvider CreateServiceProvider(IServiceCollection collection) =>
        collection.BuildServiceProvider(ServiceProviderMode.Default);
}