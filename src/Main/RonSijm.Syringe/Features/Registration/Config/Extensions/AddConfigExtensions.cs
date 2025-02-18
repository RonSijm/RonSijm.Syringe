using Microsoft.Extensions.Configuration;

namespace RonSijm.Syringe;

public static class AddConfigExtensions
{
    public static SyringeServiceCollection WithConfig(this SyringeServiceCollection services, string settingsFile = "appsettings.json", string section = "DependencyInjection")
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile(settingsFile)
            .Build();

        return WithConfig(services, config, section);
    }

    public static SyringeServiceCollection WithConfig(this SyringeServiceCollection services, IConfigurationRoot config, string session = "DependencyInjection")
    {
        var diConfig = GetDIConfig(config, session);

        return WithConfig(services, diConfig);
    }

    public static SyringeServiceCollection WithConfig(this SyringeServiceCollection services, DependencyInjectionConfig config)
    {
        services.Extensions.Add(new RegistrationsFromConfigSyringeServiceDescriptorExtension(config));
        return services;
    }

    public static DependencyInjectionConfig GetDIConfig(this IConfigurationRoot config, string session = "DependencyInjection")
    {
        var diConfig = new DependencyInjectionConfig();
        config.GetSection(session).Bind(diConfig);
        return diConfig;
    }
}