// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;

namespace MicrosoftCopy.DependencyInjection.Tests.Fakes;

public struct StructService
{
    public StructService(IServiceScopeFactory scopeFactory)
    {
    }
}