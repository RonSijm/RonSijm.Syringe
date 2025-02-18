#if NETFRAMEWORK || NETSTANDARD2_0
using System.Runtime.Serialization;
#else
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace RonSijm.Syringe.Common;

internal static partial class ParameterDefaultValue
{
    public static bool CheckHasDefaultValue(ParameterInfo parameter, out bool tryToGetDefaultValue)
    {
        tryToGetDefaultValue = true;
        return parameter.HasDefaultValue;
    }
}

#endif