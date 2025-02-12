// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace RonSijm.Syringe.ServiceLookup;

internal abstract class ServiceProviderEngine
{
    public abstract Func<ServiceProviderEngineScope, object> RealizeService(ServiceCallSite callSite);
}