﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe.ServiceLookup;

public readonly struct ServiceIdentifier : IEquatable<ServiceIdentifier>
{
    public object ServiceKey { get; }

    public Type ServiceType { get; }

    public ServiceIdentifier(Type serviceType)
    {
        ServiceType = serviceType;
    }

    public ServiceIdentifier(object serviceKey, Type serviceType)
    {
        ServiceKey = serviceKey;
        ServiceType = serviceType;
    }

    public static ServiceIdentifier FromDescriptor(ServiceDescriptor serviceDescriptor) => new(serviceDescriptor.ServiceKey, serviceDescriptor.ServiceType);

    public static ServiceIdentifier FromServiceType(Type type) => new(null, type);

    public bool Equals(ServiceIdentifier other)
    {
        if (ServiceKey == null && other.ServiceKey == null)
        {
            return ServiceType == other.ServiceType;
        }

        if (ServiceKey != null && other.ServiceKey != null)
        {
            return ServiceType == other.ServiceType && ServiceKey.Equals(other.ServiceKey);
        }
        return false;
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        return obj is ServiceIdentifier && Equals((ServiceIdentifier)obj);
    }

    public override int GetHashCode()
    {
        if (ServiceKey == null)
        {
            return ServiceType.GetHashCode();
        }
        unchecked
        {
            return (ServiceType.GetHashCode() * 397) ^ ServiceKey.GetHashCode();
        }
    }

    public bool IsConstructedGenericType => ServiceType.IsConstructedGenericType;

    public ServiceIdentifier GetGenericTypeDefinition() => new(ServiceKey, ServiceType.GetGenericTypeDefinition());

    public override string ToString()
    {
        if (ServiceKey == null)
        {
            return ServiceType.ToString();
        }

        return $"({ServiceKey}, {ServiceType})";
    }
}