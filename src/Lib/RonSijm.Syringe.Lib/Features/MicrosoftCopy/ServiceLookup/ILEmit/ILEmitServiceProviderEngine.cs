// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace RonSijm.Syringe.ServiceLookup.ILEmit;

internal sealed class ILEmitServiceProviderEngine : ServiceProviderEngine
{
    private readonly ILEmitResolverBuilder _expressionResolverBuilder;

    [RequiresDynamicCode("Creates DynamicMethods")]
    public ILEmitServiceProviderEngine(MicrosoftServiceProvider serviceProvider)
    {
        _expressionResolverBuilder = new ILEmitResolverBuilder(serviceProvider);
    }

    public override Func<ServiceProviderEngineScope, object> RealizeService(ServiceCallSite callSite)
    {
        return _expressionResolverBuilder.Build(callSite);
    }
}