// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Reflection;

namespace MicrosoftCopy.DependencyInjection.Tests.Utils;

public static class MultiServiceHelpers
{
    public static IEnumerable GetMultiService(Type collectionType, Func<Type, IEnumerable> getAllServices)
    {
        if (IsGenericIEnumerable(collectionType))
        {
            var serviceType = FirstGenericArgument(collectionType);
            return Cast(getAllServices(serviceType), serviceType);
        }

        return null;
    }

    private static IEnumerable Cast(IEnumerable collection, Type castItemsTo)
    {
        var castedCollection = CreateEmptyList(castItemsTo);

        foreach (var item in collection)
        {
            castedCollection.Add(item);
        }

        return castedCollection;
    }

    private static bool IsGenericIEnumerable(Type type)
    {
        return type.GetTypeInfo().IsGenericType &&
               type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
    }

    private static Type FirstGenericArgument(Type type)
    {
        return type.GetTypeInfo().GenericTypeArguments[0];
    }

    private static IList CreateEmptyList(Type innerType)
    {
        var listType = typeof(List<>).MakeGenericType(innerType);
        return (IList)Activator.CreateInstance(listType);
    }
}