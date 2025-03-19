using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.Settings;

namespace RonSijm.Syringe;

public class SyringeServiceDescriptor
{
    public Assembly Assembly { get; init; }
    public ServiceLifetime ServiceLifetime { get; set; } = SyringeGlobalSettings.DefaultServiceLifetime;
    public List<RegistrationSettingBase> RegistrationSettings { get; set; }
}