// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.Common;
using SR = RonSijm.Syringe.Common.SR;

namespace RonSijm.Syringe.ServiceLookup;

public sealed class CallSiteFactory : IServiceProviderIsKeyedService
{
    private const int DefaultSlot = 0;
    private readonly List<ServiceDescriptor> _descriptors;
    public ConcurrentDictionary<ServiceCacheKey, ServiceCallSite> CallSiteCache = new();
    public Dictionary<ServiceIdentifier, ServiceDescriptorCacheItem> DescriptorLookup = new();
    private readonly ConcurrentDictionary<ServiceIdentifier, object> _callSiteLocks = new();

    private readonly StackGuard _stackGuard;

    public CallSiteFactory(ICollection<ServiceDescriptor> descriptors)
    {
        _stackGuard = new StackGuard();
        _descriptors = new List<ServiceDescriptor>();
        AddDescriptors(descriptors);
    }

    public void AddDescriptors(ICollection<ServiceDescriptor> descriptors)
    {
        _descriptors.AddRange(descriptors);
        Populate(_descriptors);
    }

    internal List<ServiceDescriptor> Descriptors => _descriptors;

    internal void Populate()
    {
        Populate(_descriptors);
    }

    internal void Populate(List<ServiceDescriptor> descriptors)
    {
        foreach (var descriptor in descriptors)
        {
            var serviceType = descriptor.ServiceType;
            if (serviceType.IsGenericTypeDefinition)
            {
                var implementationType = descriptor.GetImplementationType();

                if (implementationType == null || !implementationType.IsGenericTypeDefinition)
                {
                    throw new ArgumentException(
                        SR.Format(SR.OpenGenericServiceRequiresOpenGenericImplementation, serviceType),
                        "descriptors");
                }

                if (implementationType.IsAbstract || implementationType.IsInterface)
                {
                    throw new ArgumentException(
                        SR.Format(SR.TypeCannotBeActivated, implementationType, serviceType));
                }

                var serviceTypeGenericArguments = serviceType.GetGenericArguments();
                var implementationTypeGenericArguments = implementationType.GetGenericArguments();
                if (serviceTypeGenericArguments.Length != implementationTypeGenericArguments.Length)
                {
                    throw new ArgumentException(
                        SR.Format(SR.ArityOfOpenGenericServiceNotEqualArityOfOpenGenericImplementation, serviceType, implementationType), "descriptors");
                }

                if (MicrosoftServiceProvider.VerifyOpenGenericServiceTrimmability)
                {
                    ValidateTrimmingAnnotations(serviceType, serviceTypeGenericArguments, implementationType, implementationTypeGenericArguments);
                }
            }
            else if (descriptor.TryGetImplementationType(out var implementationType))
            {
                Debug.Assert(implementationType != null);

                if (implementationType.IsGenericTypeDefinition ||
                    implementationType.IsAbstract ||
                    implementationType.IsInterface)
                {
                    throw new ArgumentException(
                        SR.Format(SR.TypeCannotBeActivated, implementationType, serviceType));
                }
            }

            var cacheKey = ServiceIdentifier.FromDescriptor(descriptor);
            DescriptorLookup.TryGetValue(cacheKey, out var cacheItem);
            DescriptorLookup[cacheKey] = cacheItem.Add(descriptor);
        }
    }

    /// <summary>
    /// Validates that two generic type definitions have compatible trimming annotations on their generic arguments.
    /// </summary>
    /// <remarks>
    /// When open generic types are used in DI, there is an error when the concrete implementation type
    /// has [DynamicallyAccessedMembers] attributes on a generic argument type, but the interface/service type
    /// doesn't have matching annotations. The problem is that the trimmer doesn't see the members that need to
    /// be preserved on the type being passed to the generic argument. But when the interface/service type also has
    /// the annotations, the trimmer will see which members need to be preserved on the closed generic argument type.
    /// </remarks>
    private static void ValidateTrimmingAnnotations(
        Type serviceType,
        Type[] serviceTypeGenericArguments,
        Type implementationType,
        Type[] implementationTypeGenericArguments)
    {
        Debug.Assert(serviceTypeGenericArguments.Length == implementationTypeGenericArguments.Length);

        for (var i = 0; i < serviceTypeGenericArguments.Length; i++)
        {
            var serviceGenericType = serviceTypeGenericArguments[i];
            var implementationGenericType = implementationTypeGenericArguments[i];

            var serviceDynamicallyAccessedMembers = GetDynamicallyAccessedMemberTypes(serviceGenericType);
            var implementationDynamicallyAccessedMembers = GetDynamicallyAccessedMemberTypes(implementationGenericType);

            if (!AreCompatible(serviceDynamicallyAccessedMembers, implementationDynamicallyAccessedMembers))
            {
                throw new ArgumentException(SR.Format(SR.TrimmingAnnotationsDoNotMatch, implementationType.FullName, serviceType.FullName));
            }

            var serviceHasNewConstraint = serviceGenericType.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint);
            var implementationHasNewConstraint = implementationGenericType.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint);
            if (implementationHasNewConstraint && !serviceHasNewConstraint)
            {
                throw new ArgumentException(SR.Format(SR.TrimmingAnnotationsDoNotMatch_NewConstraint, implementationType.FullName, serviceType.FullName));
            }
        }
    }

    private static DynamicallyAccessedMemberTypes GetDynamicallyAccessedMemberTypes(Type serviceGenericType)
    {
        foreach (var attributeData in serviceGenericType.GetCustomAttributesData())
        {
            if (attributeData.AttributeType.FullName == "System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembersAttribute" &&
                attributeData.ConstructorArguments.Count == 1 &&
                attributeData.ConstructorArguments[0].ArgumentType.FullName == "System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes")
            {
                return (DynamicallyAccessedMemberTypes)(int)attributeData.ConstructorArguments[0].Value!;
            }
        }

        return DynamicallyAccessedMemberTypes.None;
    }

    private static bool AreCompatible(DynamicallyAccessedMemberTypes serviceDynamicallyAccessedMembers, DynamicallyAccessedMemberTypes implementationDynamicallyAccessedMembers)
    {
        // The DynamicallyAccessedMemberTypes don't need to exactly match.
        // The service type needs to preserve a superset of the members required by the implementation type.
        return serviceDynamicallyAccessedMembers.HasFlag(implementationDynamicallyAccessedMembers);
    }

    // For unit testing
    internal int? GetSlot(ServiceDescriptor serviceDescriptor)
    {
        if (DescriptorLookup.TryGetValue(ServiceIdentifier.FromDescriptor(serviceDescriptor), out var item))
        {
            return item.GetSlot(serviceDescriptor);
        }

        return null;
    }

    /// <summary>
    /// Gets the slot for a descriptor when querying with AnyKey.
    /// This looks up the slot based on the descriptor's actual key (not AnyKey)
    /// so that singleton instances are shared with direct key queries.
    /// </summary>
    private int GetSlotForDescriptor(ServiceDescriptor descriptor)
    {
        // Use ServiceIdentifier.FromDescriptor to match how DescriptorLookup is keyed
        var identifier = ServiceIdentifier.FromDescriptor(descriptor);
        if (DescriptorLookup.TryGetValue(identifier, out var item))
        {
            return item.GetSlot(descriptor);
        }
        // Fallback to DefaultSlot if not found in lookup
        return DefaultSlot;
    }

    internal ServiceCallSite GetCallSite(ServiceIdentifier serviceIdentifier, CallSiteChain callSiteChain) =>
        CallSiteCache.TryGetValue(new ServiceCacheKey(serviceIdentifier, DefaultSlot), out var site) ? site :
            CreateCallSite(serviceIdentifier, callSiteChain);

    internal ServiceCallSite GetCallSite(ServiceDescriptor serviceDescriptor, CallSiteChain callSiteChain)
    {
        var serviceIdentifier = ServiceIdentifier.FromDescriptor(serviceDescriptor);
        if (DescriptorLookup.TryGetValue(serviceIdentifier, out var descriptor))
        {
            return TryCreateExact(serviceDescriptor, serviceIdentifier, callSiteChain, descriptor.GetSlot(serviceDescriptor));
        }

        Debug.Fail("_descriptorLookup didn't contain requested serviceDescriptor");
        return null;
    }

    private ServiceCallSite CreateCallSite(ServiceIdentifier serviceIdentifier, CallSiteChain callSiteChain)
    {
        if (!_stackGuard.TryEnterOnCurrentStack())
        {
            return _stackGuard.RunOnEmptyStack(CreateCallSite, serviceIdentifier, callSiteChain);
        }

        // We need to lock the resolution process for a single service type at a time:
        // Consider the following:
        // C -> D -> A
        // E -> D -> A
        // Resolving C and E in parallel means that they will be modifying the callsite cache concurrently
        // to add the entry for C and E, but the resolution of D and A is synchronized
        // to make sure C and E both reference the same instance of the callsite.

        // This is to make sure we can safely store singleton values on the callsites themselves

        var callsiteLock = _callSiteLocks.GetOrAdd(serviceIdentifier, static _ => new object());

        lock (callsiteLock)
        {
            callSiteChain.CheckCircularDependency(serviceIdentifier);

            var callSite = TryCreateExact(serviceIdentifier, callSiteChain) ??
                           TryCreateOpenGeneric(serviceIdentifier, callSiteChain) ??
                           TryCreateEnumerable(serviceIdentifier, callSiteChain);

            return callSite;
        }
    }

    private ServiceCallSite TryCreateExact(ServiceIdentifier serviceIdentifier, CallSiteChain callSiteChain)
    {
        if (DescriptorLookup.TryGetValue(serviceIdentifier, out var descriptor))
        {
            return TryCreateExact(descriptor.Last, serviceIdentifier, callSiteChain, DefaultSlot);
        }

        if (serviceIdentifier.ServiceKey != null)
        {
            // Check if there is a registration with KeyedService.AnyKey
            var catchAllIdentifier = new ServiceIdentifier(KeyedService.AnyKey, serviceIdentifier.ServiceType);
            if (DescriptorLookup.TryGetValue(catchAllIdentifier, out descriptor))
            {
                return TryCreateExact(descriptor.Last, serviceIdentifier, callSiteChain, DefaultSlot);
            }
        }

        return null;
    }

    private ServiceCallSite TryCreateOpenGeneric(ServiceIdentifier serviceIdentifier, CallSiteChain callSiteChain)
    {
        if (serviceIdentifier.IsConstructedGenericType)
        {
            var genericIdentifier = serviceIdentifier.GetGenericTypeDefinition();
            if (DescriptorLookup.TryGetValue(genericIdentifier, out var descriptor))
            {
                return TryCreateOpenGeneric(descriptor.Last, serviceIdentifier, callSiteChain, DefaultSlot, true);
            }

            if (serviceIdentifier.ServiceKey != null)
            {
                // Check if there is a registration with KeyedService.AnyKey
                var catchAllIdentifier = new ServiceIdentifier(KeyedService.AnyKey, genericIdentifier.ServiceType);
                if (DescriptorLookup.TryGetValue(catchAllIdentifier, out descriptor))
                {
                    return TryCreateOpenGeneric(descriptor.Last, serviceIdentifier, callSiteChain, DefaultSlot, true);
                }
            }
        }

        return null;
    }

    private ServiceCallSite TryCreateEnumerable(ServiceIdentifier serviceIdentifier, CallSiteChain callSiteChain)
    {
        var callSiteKey = new ServiceCacheKey(serviceIdentifier, DefaultSlot);
        if (CallSiteCache.TryGetValue(callSiteKey, out var serviceCallSite))
        {
            return serviceCallSite;
        }

        try
        {
            callSiteChain.Add(serviceIdentifier);

            var serviceType = serviceIdentifier.ServiceType;

            if (!serviceType.IsConstructedGenericType ||
                serviceType.GetGenericTypeDefinition() != typeof(IEnumerable<>))
            {
                return null;
            }

            var itemType = serviceType.GenericTypeArguments[0];
            var cacheKey = new ServiceIdentifier(serviceIdentifier.ServiceKey, itemType);
            if (MicrosoftServiceProvider.VerifyAotCompatibility && itemType.IsValueType)
            {
                // NativeAOT apps are not able to make Enumerable of ValueType services
                // since there is no guarantee the ValueType[] code has been generated.
                throw new InvalidOperationException(SR.Format(SR.AotCannotCreateEnumerableValueType, itemType));
            }

            var cacheLocation = CallSiteResultCacheLocation.Root;
            ServiceCallSite[] callSites;

            // If item type is not generic we can safely use descriptor cache
            // Special case for KeyedService.AnyKey, we don't want to check the cache because a KeyedService.AnyKey registration
            // will "hide" all the other service registration
            if (!itemType.IsConstructedGenericType &&
                !KeyedService.AnyKey.Equals(cacheKey.ServiceKey) &&
                DescriptorLookup.TryGetValue(cacheKey, out var descriptors))
            {
                callSites = new ServiceCallSite[descriptors.Count];
                for (var i = 0; i < descriptors.Count; i++)
                {
                    var descriptor = descriptors[i];

                    // Last service should get slot 0
                    var slot = descriptors.Count - i - 1;
                    // There may not be any open generics here
                    var callSite = TryCreateExact(descriptor, cacheKey, callSiteChain, slot);
                    Debug.Assert(callSite != null);

                    cacheLocation = GetCommonCacheLocation(cacheLocation, callSite.Cache.Location);
                    callSites[i] = callSite;
                }
            }
            else
            {
                // We need to construct a list of matching call sites in declaration order, but to ensure
                // correct caching we must assign slots in reverse declaration order and with slots being
                // given out first to any exact matches before any open generic matches. Therefore, we
                // iterate over the descriptors twice in reverse, catching exact matches on the first pass
                // and open generic matches on the second pass.

                List<KeyValuePair<int, ServiceCallSite>> callSitesByIndex = new();

                var slot = 0;
                // When querying with AnyKey, we need to use the descriptor's actual key for caching
                // so that singleton instances are shared with direct key queries.
                // We also need to compute the correct slot for each descriptor based on how many
                // registrations exist for that specific key.
                var isAnyKeyQuery = KeyedService.AnyKey.Equals(cacheKey.ServiceKey);

                for (var i = _descriptors.Count - 1; i >= 0; i--)
                {
                    if (KeysMatchForEnumerable(_descriptors[i].ServiceKey, cacheKey.ServiceKey))
                    {
                        var effectiveIdentifier = isAnyKeyQuery
                            ? new ServiceIdentifier(_descriptors[i].ServiceKey, itemType)
                            : cacheKey;
                        // For AnyKey queries, compute the slot based on the descriptor's actual key
                        // to match what would be used for a direct query
                        var effectiveSlot = isAnyKeyQuery
                            ? GetSlotForDescriptor(_descriptors[i])
                            : slot;
                        if (TryCreateExact(_descriptors[i], effectiveIdentifier, callSiteChain, effectiveSlot) is { } callSite)
                        {
                            AddCallSite(callSite, i);
                        }
                    }
                }
                for (var i = _descriptors.Count - 1; i >= 0; i--)
                {
                    if (KeysMatchForEnumerable(_descriptors[i].ServiceKey, cacheKey.ServiceKey))
                    {
                        var effectiveIdentifier = isAnyKeyQuery
                            ? new ServiceIdentifier(_descriptors[i].ServiceKey, itemType)
                            : cacheKey;
                        // For AnyKey queries, compute the slot based on the descriptor's actual key
                        // to match what would be used for a direct query
                        var effectiveSlot = isAnyKeyQuery
                            ? GetSlotForDescriptor(_descriptors[i])
                            : slot;
                        if (TryCreateOpenGeneric(_descriptors[i], effectiveIdentifier, callSiteChain, effectiveSlot, throwOnConstraintViolation: false) is { } callSite)
                        {
                            AddCallSite(callSite, i);
                        }
                    }
                }

                callSitesByIndex.Sort((a, b) => a.Key.CompareTo(b.Key));
                callSites = new ServiceCallSite[callSitesByIndex.Count];
                for (var i = 0; i < callSites.Length; ++i)
                {
                    callSites[i] = callSitesByIndex[i].Value;
                }

                void AddCallSite(ServiceCallSite callSite, int index)
                {
                    slot++;

                    cacheLocation = GetCommonCacheLocation(cacheLocation, callSite.Cache.Location);
                    callSitesByIndex.Add(new(index, callSite));
                }
            }
            var resultCache = (cacheLocation == CallSiteResultCacheLocation.Scope || cacheLocation == CallSiteResultCacheLocation.Root)
                ? new ResultCache(cacheLocation, callSiteKey)
                : new ResultCache(CallSiteResultCacheLocation.None, callSiteKey);
            return CallSiteCache[callSiteKey] = new IEnumerableCallSite(resultCache, itemType, callSites);
        }
        finally
        {
            callSiteChain.Remove(serviceIdentifier);
        }
    }

    private static CallSiteResultCacheLocation GetCommonCacheLocation(CallSiteResultCacheLocation locationA, CallSiteResultCacheLocation locationB)
    {
        return (CallSiteResultCacheLocation)Math.Max((int)locationA, (int)locationB);
    }

    private ServiceCallSite TryCreateExact(ServiceDescriptor descriptor, ServiceIdentifier serviceIdentifier, CallSiteChain callSiteChain, int slot)
    {
        if (serviceIdentifier.ServiceType == descriptor.ServiceType)
        {
            var callSiteKey = new ServiceCacheKey(serviceIdentifier, slot);
            if (CallSiteCache.TryGetValue(callSiteKey, out var serviceCallSite))
            {
                return serviceCallSite;
            }

            ServiceCallSite callSite;
            var lifetime = new ResultCache(descriptor.Lifetime, serviceIdentifier, slot);
            if (descriptor.HasImplementationInstance())
            {
                callSite = new ConstantCallSite(descriptor.ServiceType, descriptor.GetImplementationInstance());
            }
            else if (!descriptor.IsKeyedService && descriptor.ImplementationFactory != null)
            {
                callSite = new FactoryCallSite(lifetime, descriptor.ServiceType, descriptor.ImplementationFactory);
            }
            else if (descriptor.IsKeyedService && descriptor.KeyedImplementationFactory != null)
            {
                callSite = new FactoryCallSite(lifetime, descriptor.ServiceType, serviceIdentifier.ServiceKey!, descriptor.KeyedImplementationFactory);
            }
            else if (descriptor.HasImplementationType())
            {
                callSite = CreateConstructorCallSite(lifetime, serviceIdentifier, descriptor.GetImplementationType()!, callSiteChain);
            }
            else
            {
                throw new InvalidOperationException(SR.InvalidServiceDescriptor);
            }
            callSite.Key = descriptor.ServiceKey;

            return CallSiteCache[callSiteKey] = callSite;
        }

        return null;
    }

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2055:MakeGenericType",
        Justification = "MakeGenericType here is used to create a closed generic implementation type given the closed service type. " +
                        "Trimming annotations on the generic types are verified when 'Microsoft.Extensions.DependencyInjection.VerifyOpenGenericServiceTrimmability' is set, which is set by default when PublishTrimmed=true. " +
                        "That check informs developers when these generic types don't have compatible trimming annotations.")]
    [UnconditionalSuppressMessage("AotAnalysis", "IL3050:RequiresDynamicCode",
        Justification = "When ServiceProvider.VerifyAotCompatibility is true, which it is by default when PublishAot=true, " +
                        "this method ensures the generic types being created aren't using ValueTypes.")]
    private ServiceCallSite TryCreateOpenGeneric(ServiceDescriptor descriptor, ServiceIdentifier serviceIdentifier, CallSiteChain callSiteChain, int slot, bool throwOnConstraintViolation)
    {
        if (serviceIdentifier.IsConstructedGenericType &&
            serviceIdentifier.ServiceType.GetGenericTypeDefinition() == descriptor.ServiceType)
        {
            var callSiteKey = new ServiceCacheKey(serviceIdentifier, slot);
            if (CallSiteCache.TryGetValue(callSiteKey, out var serviceCallSite))
            {
                return serviceCallSite;
            }

            var implementationType = descriptor.GetImplementationType();
            Debug.Assert(implementationType != null, "descriptor.ImplementationType != null");
            var lifetime = new ResultCache(descriptor.Lifetime, serviceIdentifier, slot);
            Type closedType;
            try
            {
                var genericTypeArguments = serviceIdentifier.ServiceType.GenericTypeArguments;
                if (MicrosoftServiceProvider.VerifyAotCompatibility)
                {
                    VerifyOpenGenericAotCompatibility(serviceIdentifier.ServiceType, genericTypeArguments);
                }

                closedType = implementationType.MakeGenericType(genericTypeArguments);
            }
            catch (ArgumentException)
            {
                if (throwOnConstraintViolation)
                {
                    throw;
                }

                return null;
            }

            return CallSiteCache[callSiteKey] = CreateConstructorCallSite(lifetime, serviceIdentifier, closedType, callSiteChain);
        }

        return null;
    }

    private ConstructorCallSite CreateConstructorCallSite(
        ResultCache lifetime,
        ServiceIdentifier serviceIdentifier,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType,
        CallSiteChain callSiteChain)
    {
        try
        {
            callSiteChain.Add(serviceIdentifier, implementationType);
            var constructors = implementationType.GetConstructors();

            ServiceCallSite[] parameterCallSites = null;

            if (constructors.Length == 0)
            {
                throw new InvalidOperationException(SR.Format(SR.NoConstructorMatch, implementationType));
            }
            else if (constructors.Length == 1)
            {
                var constructor = constructors[0];
                var parameters = constructor.GetParameters();
                if (parameters.Length == 0)
                {
                    return new ConstructorCallSite(lifetime, serviceIdentifier.ServiceType, constructor);
                }

                parameterCallSites = CreateArgumentCallSites(
                    serviceIdentifier,
                    implementationType,
                    callSiteChain,
                    parameters,
                    throwIfCallSiteNotFound: true)!;

                return new ConstructorCallSite(lifetime, serviceIdentifier.ServiceType, constructor, parameterCallSites);
            }

            Array.Sort(constructors,
                (a, b) => b.GetParameters().Length.CompareTo(a.GetParameters().Length));

            ConstructorInfo bestConstructor = null;
            HashSet<Type> bestConstructorParameterTypes = null;
            for (var i = 0; i < constructors.Length; i++)
            {
                var parameters = constructors[i].GetParameters();

                var currentParameterCallSites = CreateArgumentCallSites(
                    serviceIdentifier,
                    implementationType,
                    callSiteChain,
                    parameters,
                    throwIfCallSiteNotFound: false);

                if (currentParameterCallSites != null)
                {
                    if (bestConstructor == null)
                    {
                        bestConstructor = constructors[i];
                        parameterCallSites = currentParameterCallSites;
                    }
                    else
                    {
                        // Since we're visiting constructors in decreasing order of number of parameters,
                        // we'll only see ambiguities or supersets once we've seen a 'bestConstructor'.

                        if (bestConstructorParameterTypes == null)
                        {
                            bestConstructorParameterTypes = new HashSet<Type>();
                            foreach (var p in bestConstructor.GetParameters())
                            {
                                bestConstructorParameterTypes.Add(p.ParameterType);
                            }
                        }

                        foreach (var p in parameters)
                        {
                            if (!bestConstructorParameterTypes.Contains(p.ParameterType))
                            {
                                // Ambiguous match exception
                                throw new InvalidOperationException(string.Join(
                                    Environment.NewLine,
                                    SR.Format(SR.AmbiguousConstructorException, implementationType),
                                    bestConstructor,
                                    constructors[i]));
                            }
                        }
                    }
                }
            }

            if (bestConstructor == null)
            {
                throw new InvalidOperationException(
                    SR.Format(SR.UnableToActivateTypeException, implementationType));
            }
            else
            {
                Debug.Assert(parameterCallSites != null);
                return new ConstructorCallSite(lifetime, serviceIdentifier.ServiceType, bestConstructor, parameterCallSites);
            }
        }
        finally
        {
            callSiteChain.Remove(serviceIdentifier);
        }
    }

    /// <returns>Not <b>null</b> if <b>throwIfCallSiteNotFound</b> is true</returns>
    private ServiceCallSite[] CreateArgumentCallSites(
        ServiceIdentifier serviceIdentifier,
        Type implementationType,
        CallSiteChain callSiteChain,
        ParameterInfo[] parameters,
        bool throwIfCallSiteNotFound)
    {
        var parameterCallSites = new ServiceCallSite[parameters.Length];

        for (var index = 0; index < parameters.Length; index++)
        {
            ServiceCallSite callSite = null;
            var isKeyedParameter = false;
            var parameterType = parameters[index].ParameterType;
            foreach (var attribute in parameters[index].GetCustomAttributes(true))
            {
                if (serviceIdentifier.ServiceKey != null && attribute is ServiceKeyAttribute)
                {
                    // Check if the parameter type matches
                    if (parameterType != serviceIdentifier.ServiceKey.GetType())
                    {
                        throw new InvalidOperationException(SR.InvalidServiceKeyType);
                    }
                    callSite = new ConstantCallSite(parameterType, serviceIdentifier.ServiceKey);
                    break;
                }
                if (attribute is FromKeyedServicesAttribute keyed)
                {
                    var parameterSvcId = new ServiceIdentifier(keyed.Key, parameterType);
                    callSite = GetCallSite(parameterSvcId, callSiteChain);
                    isKeyedParameter = true;
                    break;
                }
            }

            if (!isKeyedParameter)
            {
                callSite ??= GetCallSite(ServiceIdentifier.FromServiceType(parameterType), callSiteChain);

                // If no callsite found, try to find a keyed service where the key matches the parameter name
                if (callSite == null)
                {
                    var parameterName = parameters[index].Name;
                    if (parameterName != null)
                    {
                        var keyedServiceIdentifier = new ServiceIdentifier(parameterName, parameterType);
                        callSite = GetCallSite(keyedServiceIdentifier, callSiteChain);
                    }
                }
            }

            if (callSite == null && ParameterDefaultValue.TryGetDefaultValue(parameters[index], out var defaultValue))
            {
                callSite = new ConstantCallSite(parameterType, defaultValue);
            }

            if (callSite == null)
            {
                if (throwIfCallSiteNotFound)
                {
                    throw new InvalidOperationException(SR.Format(SR.CannotResolveService,
                        parameterType,
                        implementationType));
                }

                return null;
            }

            parameterCallSites[index] = callSite;
        }

        return parameterCallSites;
    }

    /// <summary>
    /// Verifies none of the generic type arguments are ValueTypes.
    /// </summary>
    /// <remarks>
    /// NativeAOT apps are not guaranteed that the native code for the closed generic of ValueType
    /// has been generated. To catch these problems early, this verification is enabled at development-time
    /// to inform the developer early that this scenario will not work once AOT'd.
    /// </remarks>
    private static void VerifyOpenGenericAotCompatibility(Type serviceType, Type[] genericTypeArguments)
    {
        foreach (var typeArg in genericTypeArguments)
        {
            if (typeArg.IsValueType)
            {
                throw new InvalidOperationException(SR.Format(SR.AotCannotCreateGenericValueType, serviceType, typeArg));
            }
        }
    }

    public void Add(ServiceIdentifier serviceIdentifier, ServiceCallSite serviceCallSite)
    {
        CallSiteCache[new ServiceCacheKey(serviceIdentifier, DefaultSlot)] = serviceCallSite;
    }

    public bool IsService(Type serviceType) => IsService(new ServiceIdentifier(null, serviceType));

    public bool IsKeyedService(Type serviceType, object key) => IsService(new ServiceIdentifier(key, serviceType));

    internal bool IsService(ServiceIdentifier serviceIdentifier)
    {
        var serviceType = serviceIdentifier.ServiceType;

        if (serviceType is null)
        {
            throw new ArgumentNullException(nameof(serviceType));
        }

        // Querying for an open generic should return false (they aren't resolvable)
        if (serviceType.IsGenericTypeDefinition)
        {
            return false;
        }

        if (DescriptorLookup.ContainsKey(serviceIdentifier))
        {
            return true;
        }

        if (serviceIdentifier.ServiceKey != null && DescriptorLookup.ContainsKey(new ServiceIdentifier(KeyedService.AnyKey, serviceType)))
        {
            return true;
        }

        if (serviceType.IsConstructedGenericType && serviceType.GetGenericTypeDefinition() is { } genericDefinition)
        {
            // We special case IEnumerable since it isn't explicitly registered in the container
            // yet we can manifest instances of it when requested.
            return genericDefinition == typeof(IEnumerable<>) || DescriptorLookup.ContainsKey(serviceIdentifier.GetGenericTypeDefinition());
        }

        // These are the built in service types that aren't part of the list of service descriptors
        // If you update these make sure to also update the code in ServiceProvider.ctor
        return serviceType == typeof(IServiceProvider) ||
               serviceType == typeof(IServiceScopeFactory) ||
               serviceType == typeof(IServiceProviderIsService) ||
               serviceType == typeof(IServiceProviderIsKeyedService);
    }

    /// <summary>
    /// Returns true if both keys are null or equals, or if key1 is KeyedService.AnyKey and key2 is not null
    /// </summary>
    private static bool KeysMatch(object key1, object key2)
    {
        if (key1 == null && key2 == null)
            return true;

        if (key1 != null && key2 != null)
        {
            return key1.Equals(key2)
                   || key1.Equals(KeyedService.AnyKey)
                   || key2.Equals(KeyedService.AnyKey);
        }

        return false;
    }

    /// <summary>
    /// Returns true if the descriptor key matches the query key for enumerable resolution.
    /// For enumerable resolution:
    /// - AnyKey registrations are NEVER included in the results
    /// - When querying with AnyKey, return all services with non-null keys (but not AnyKey registrations)
    /// - When querying with a specific key, return only services with that exact key
    /// </summary>
    private static bool KeysMatchForEnumerable(object descriptorKey, object queryKey)
    {
        // AnyKey registrations are never included in enumerable results
        if (KeyedService.AnyKey.Equals(descriptorKey))
            return false;

        // If querying with AnyKey, match all services with non-null keys
        if (KeyedService.AnyKey.Equals(queryKey))
        {
            return descriptorKey != null;
        }

        // For specific key queries, only match exact keys (or both null)
        if (descriptorKey == null && queryKey == null)
            return true;

        if (descriptorKey != null && queryKey != null)
        {
            return descriptorKey.Equals(queryKey);
        }

        return false;
    }

    public struct ServiceDescriptorCacheItem
    {
        [DisallowNull]
        private ServiceDescriptor _item;

        [DisallowNull]
        private List<ServiceDescriptor> _items;

        public ServiceDescriptor Last
        {
            get
            {
                if (_items != null && _items.Count > 0)
                {
                    return _items[_items.Count - 1];
                }

                Debug.Assert(_item != null);
                return _item;
            }
        }

        public int Count
        {
            get
            {
                if (_item == null)
                {
                    Debug.Assert(_items == null);
                    return 0;
                }

                return 1 + (_items?.Count ?? 0);
            }
        }

        public ServiceDescriptor this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);

                if (index == 0)
                {
                    return _item!;
                }

                return _items![index - 1];
            }
        }

        public int GetSlot(ServiceDescriptor descriptor)
        {
            if (descriptor == _item)
            {
                return Count - 1;
            }

            if (_items != null)
            {
                var index = _items.IndexOf(descriptor);
                if (index != -1)
                {
                    return _items.Count - (index + 1);
                }
            }

            throw new InvalidOperationException(SR.ServiceDescriptorNotExist);
        }

        public ServiceDescriptorCacheItem Add(ServiceDescriptor descriptor)
        {
            var newCacheItem = default(ServiceDescriptorCacheItem);
            if (_item == null)
            {
                Debug.Assert(_items == null);
                newCacheItem._item = descriptor;
            }
            else
            {
                newCacheItem._item = _item;
                newCacheItem._items = _items ?? new List<ServiceDescriptor>();
                newCacheItem._items.Add(descriptor);
            }
            return newCacheItem;
        }
    }
}