namespace RonSijm.Syringe;

public class RegistrationsFromConfigSyringeServiceDescriptorExtension(DependencyInjectionConfig config) : IWireSyringeServiceDescriptorExtension
{
    public void BeforeBuildServiceProvider(SyringeServiceDescriptor registration)
    {
        var assemblyName = registration.Assembly.GetName().Name;

        if (config == null || !config.Assembly.TryGetValue(assemblyName, out var value))
        {
            return;
        }

        registration.RegistrationSettings ??= [];

        foreach (var typeSettings in value.Type)
        {
            registration.RegistrationSettings.Add(new TypeNameRegistrationSetting
            {
                TypeName = typeSettings.Key,
                RegistrationType = typeSettings.Value
            });
        }
    }
}