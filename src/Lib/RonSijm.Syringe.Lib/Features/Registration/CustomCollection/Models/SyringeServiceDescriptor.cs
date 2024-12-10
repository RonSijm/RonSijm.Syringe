using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.Attributes;

namespace RonSijm.Syringe;

public class SyringeServiceDescriptor
{
    public Assembly Assembly { get; init; }
    public ServiceLifetime ServiceLifetime { get; set; } = SyringeGlobalSettings.DefaultServiceLifetime;
    public List<RegistrationSettingBase> RegistrationSettings { get; set; }
}

public class TypeRegistrationSetting : RegistrationSettingBase
{
    public Type Type { get; set; }
}

public class TypeNameRegistrationSetting : RegistrationSettingBase
{
    public string TypeName { get; set; }
}

public class RegistrationSettingBase
{
    public RegistrationType RegistrationType { get; set; }
}