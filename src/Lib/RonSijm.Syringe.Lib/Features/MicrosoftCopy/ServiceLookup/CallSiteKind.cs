// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace RonSijm.Syringe.ServiceLookup;

internal enum CallSiteKind
{
    Factory,

    Constructor,

    Constant,

    IEnumerable,

    ServiceProvider,
}