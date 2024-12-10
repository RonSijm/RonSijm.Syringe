// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection.Specification.Fakes;

namespace MicrosoftCopy.DependencyInjection.Tests.ServiceLookup.Types;

public class TypeWithEnumerableConstructors
{
    public TypeWithEnumerableConstructors(
        IEnumerable<IFakeService> fakeServices)
    {
    }

    public TypeWithEnumerableConstructors(
        IEnumerable<IFakeService> fakeServices,
        IEnumerable<IFactoryService> factoryServices)
    {
    }
}