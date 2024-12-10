using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.Attributes;

namespace RonSijm.Syringe;

public static class SyringeServiceProviderExtensions
{
    public static SyringeServiceCollectionAndRegistration WithLifetime(this SyringeServiceCollectionAndRegistration registration, ServiceLifetime config)
    {
        registration.Descriptor.ServiceLifetime = config;

        return registration;
    }

    public static SyringeServiceCollectionAndRegistration RegisterBothTypeAndInterfaces(this SyringeServiceCollectionAndRegistration registration, params Type[] config)
    {
        return Register(registration, RegistrationType.TypeAndInterface, config);
    }

    public static SyringeServiceCollectionAndRegistration DontRegisterTypesWithInterfaces(this SyringeServiceCollectionAndRegistration registration, params Type[] config)
    {
        return Register(registration, RegistrationType.None, config);
    }

    public static SyringeServiceCollectionAndRegistration DontRegisterTypes(this SyringeServiceCollectionAndRegistration registration, params Type[] config)
    {
        return Register(registration, RegistrationType.Interface, config);
    }

    public static SyringeServiceCollectionAndRegistration RegisterAsTypesOnly(this SyringeServiceCollectionAndRegistration registration, params Type[] config)
    {
        return Register(registration, RegistrationType.Type, config);
    }

    public static SyringeServiceCollectionAndRegistration Register(this SyringeServiceCollectionAndRegistration registration, RegistrationType registrationType, params Type[] config)
    {
        registration.Descriptor.RegistrationSettings ??= [];

        foreach (var type in config)
        {
            registration.Descriptor.RegistrationSettings.Add(new TypeRegistrationSetting() { Type = type, RegistrationType = registrationType });
        }

        return registration;
    }
}