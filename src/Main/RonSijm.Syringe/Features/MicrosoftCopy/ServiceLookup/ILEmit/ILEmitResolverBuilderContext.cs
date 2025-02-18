// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection.Emit;

namespace RonSijm.Syringe.ServiceLookup.ILEmit;

internal sealed class ILEmitResolverBuilderContext
{
    public ILEmitResolverBuilderContext(ILGenerator generator) => Generator = generator;
    public ILGenerator Generator { get; }
    public List<object> Constants { get; set; }
    public List<Func<IServiceProvider, object>> Factories { get; set; }
}