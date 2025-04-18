﻿using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.Settings;

namespace RonSijm.Syringe;

public static class ServiceCollectionExtensions
{
    public static SyringeServiceCollection WireImplicit(this IServiceCollection services, Action<SyringeServiceCollection> action)
    {
        var syringeServiceCollection = new SyringeServiceCollection(services);

        action(syringeServiceCollection);
        return syringeServiceCollection;
    }

    public static IServiceCollection WireEntryImplicit(this IServiceCollection services, ServiceLifetime defaultLifetime = ServiceLifetime.Transient,
        IReadOnlyCollection<Type> registerAsTypesOnly = null,
        IReadOnlyCollection<Type> dontRegisterTypesWithInterfaces = null,
        IReadOnlyCollection<Type> registerBothTypeAndInterfaces = null)
    {
        var assembly = Assembly.GetEntryAssembly();
        return WireImplicit(services, assembly, defaultLifetime, registerAsTypesOnly, dontRegisterTypesWithInterfaces, registerBothTypeAndInterfaces);
    }

    public static IServiceCollection WireImplicit<T>(this IServiceCollection services, ServiceLifetime defaultLifetime = ServiceLifetime.Transient,
        IReadOnlyCollection<Type> registerAsTypesOnly = null,
        IReadOnlyCollection<Type> dontRegisterTypesWithInterfaces = null,
        IReadOnlyCollection<Type> registerBothTypeAndInterfaces = null)
    {
        return WireImplicit(services, typeof(T).Assembly, defaultLifetime, registerAsTypesOnly, dontRegisterTypesWithInterfaces, registerBothTypeAndInterfaces);
    }

    public static IServiceCollection WireImplicit(this IServiceCollection services, Type targetType, ServiceLifetime defaultLifetime = ServiceLifetime.Transient,
        IReadOnlyCollection<Type> registerAsTypesOnly = null,
        IReadOnlyCollection<Type> dontRegisterTypesWithInterfaces = null,
        IReadOnlyCollection<Type> registerBothTypeAndInterfaces = null)
    {
        return WireImplicit(services, targetType.Assembly, defaultLifetime, registerAsTypesOnly, dontRegisterTypesWithInterfaces, registerBothTypeAndInterfaces);
    }

    public static IServiceCollection WireImplicit(this IServiceCollection services, Assembly targetAssembly, ServiceLifetime defaultLifetime = ServiceLifetime.Transient,
        IReadOnlyCollection<Type> registerAsTypesOnly = null,
        IReadOnlyCollection<Type> dontRegisterTypesWithInterfaces = null,
        IReadOnlyCollection<Type> registerBothTypeAndInterfaces = null,
        IReadOnlyCollection<Type> dontRegisterTypes = null)
    {
        var settingsNew = new List<RegistrationSettingBase>();

        if (registerAsTypesOnly != null)
        {
            settingsNew.AddRange(registerAsTypesOnly.Select(type => new TypeRegistrationSetting { Type = type, RegistrationType = RegistrationType.Type }));
        }

        if (dontRegisterTypesWithInterfaces != null)
        {
            settingsNew.AddRange(dontRegisterTypesWithInterfaces.Select(type => new TypeRegistrationSetting { Type = type, RegistrationType = RegistrationType.None }));
        }

        if (registerBothTypeAndInterfaces != null)
        {
            settingsNew.AddRange(registerBothTypeAndInterfaces.Select(type => new TypeRegistrationSetting { Type = type, RegistrationType = RegistrationType.TypeAndInterface }));
        }

        if (dontRegisterTypes != null)
        {
            settingsNew.AddRange(dontRegisterTypes.Select(type => new TypeRegistrationSetting { Type = type, RegistrationType = RegistrationType.None }));
        }

        return WireImplicit(services, targetAssembly, defaultLifetime, settingsNew);
    }

    public static IServiceCollection WireImplicit(this IServiceCollection services, Assembly targetAssembly, ServiceLifetime defaultLifetime = ServiceLifetime.Transient, List<RegistrationSettingBase> settings = null)
    {
        var explicitRegistrationsOfThisAssembly = services.Where(x => x.ServiceType.Assembly == targetAssembly).ToList();

        if (explicitRegistrationsOfThisAssembly.Any())
        {
            settings ??= [];

            foreach (var serviceDescriptor in explicitRegistrationsOfThisAssembly)
            {
                settings.Add(new TypeRegistrationSetting(serviceDescriptor.ServiceType, RegistrationType.Interface));
            }
        }

        var types = targetAssembly.GetTypes().Where(x => !x.IsAbstract).ToList();

        var genericTypes = types.Where(x => x.IsGenericType);

        // We register the generic types first, because if there are open generics, we register them first
        // Then if there is a more specific implementation, we overwrite it with that.
        foreach (var type in genericTypes)
        {
            var genericTypeDefinition = type.GetGenericTypeDefinition();
            services.Add(new ServiceDescriptor(genericTypeDefinition, genericTypeDefinition, defaultLifetime));

            var defaultImplementationForType = type.GetCustomAttribute<DefaultImplementationForTypeAttribute>();

            if (defaultImplementationForType != null)
            {
                services.Add(new ServiceDescriptor(defaultImplementationForType.Type, type, defaultLifetime));
            }
        }

        var nonGenericTypes = types.Where(x => !x.IsGenericType);

        foreach (var type in nonGenericTypes)
        {
            var attribute = type.GetCustomAttribute<Lifetime.ServiceLifetimeAttribute>();
            var lifetime = attribute?.ServiceLifetime ?? defaultLifetime;
            var interfaces = type.GetInterfaces();

            var registerInterfaceType = true;
            var registerAsType = true;
            var hasSettings = false;

            if (settings != null)
            {
                var settingsForType = settings.FirstOrDefault(x => GetSettings(x, type));

                if (settingsForType != null)
                {
                    registerAsType = settingsForType.RegistrationType.HasFlag(RegistrationType.Type);
                    registerInterfaceType = settingsForType.RegistrationType.HasFlag(RegistrationType.Interface);
                    hasSettings = true;
                }

                var settingsForInterfaces = settings.FirstOrDefault(x => GetSettings(x, interfaces));

                if (settingsForInterfaces != null)
                {
                    registerAsType = settingsForInterfaces.RegistrationType.HasFlag(RegistrationType.Type);
                    registerInterfaceType = settingsForInterfaces.RegistrationType.HasFlag(RegistrationType.Interface);
                    hasSettings = true;
                }
            }

            if (!hasSettings)
            {
                registerAsType = SyringeGlobalSettings.RegisterAsTypeWhenTypeHasInterfaces || interfaces.Length == 0;
            }

            if (registerAsType)
            {
                services.Add(new ServiceDescriptor(type, type, lifetime));
            }

            if (registerInterfaceType)
            {
                foreach (var typeInterface in interfaces)
                {
                    services.Add(new ServiceDescriptor(typeInterface, type, lifetime));
                }
            }
        }

        return services;
    }

    private static bool GetSettings(RegistrationSettingBase registrationSetting, params Type[] types)
    {
        if (registrationSetting is TypeRegistrationSetting typeSettings)
        {
            return types.Contains(typeSettings.Type);
        }

        if (registrationSetting is TypeNameRegistrationSetting typeNameSettings)
        {
            var matches = types.Any(x => x.Name == typeNameSettings.TypeName);
            return matches;
        }

        // Looks like you've added a new RegistrationSettingBase. Implement a way to handle it here.
        throw new NotImplementedException($"Type '{registrationSetting.GetType()}' is not supported yet.");
    }
}