// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MicrosoftCopy.DependencyInjection.Tests.Fakes;

public class DirectCircularDependencyA
{
    public DirectCircularDependencyA(DirectCircularDependencyB b)
    {

    }
}