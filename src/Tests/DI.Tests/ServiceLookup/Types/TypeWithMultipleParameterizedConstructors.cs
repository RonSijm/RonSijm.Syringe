// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection.Specification.Fakes;

namespace MicrosoftCopy.DependencyInjection.Tests.ServiceLookup.Types;

public class TypeWithMultipleParameterizedConstructors
{
    public TypeWithMultipleParameterizedConstructors(IFactoryService factoryService)
    {
    }

    public TypeWithMultipleParameterizedConstructors(IFakeService fakeService)
    {
    }
}