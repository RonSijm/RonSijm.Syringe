// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace RonSijm.Syringe.ServiceLookup.Expressions;

internal sealed class ExpressionsServiceProviderEngine : ServiceProviderEngine
{
    private readonly ExpressionResolverBuilder _expressionResolverBuilder;

    public ExpressionsServiceProviderEngine(MicrosoftServiceProvider serviceProvider)
    {
        _expressionResolverBuilder = new ExpressionResolverBuilder(serviceProvider);
    }

    public override Func<ServiceProviderEngineScope, object> RealizeService(ServiceCallSite callSite)
    {
        return _expressionResolverBuilder.Build(callSite);
    }
}