// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace RonSijm.Syringe.ServiceLookup;

/// <summary>
/// Summary description for ServiceCallSite
/// </summary>
public abstract class ServiceCallSite
{
    protected ServiceCallSite(ResultCache cache)
    {
        Cache = cache;
    }

    public abstract Type ServiceType { get; }
    public abstract Type ImplementationType { get; }
    public abstract CallSiteKind Kind { get; }
    public ResultCache Cache { get; }
    public object Value { get; set; }
    public object Key { get; set; }

    public bool CaptureDisposable =>
        ImplementationType == null ||
        typeof(IDisposable).IsAssignableFrom(ImplementationType) ||
        typeof(IAsyncDisposable).IsAssignableFrom(ImplementationType);
}