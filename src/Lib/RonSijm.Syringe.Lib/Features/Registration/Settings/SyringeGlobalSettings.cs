using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public class SyringeGlobalSettings
{
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global - Justification: Can be used to set the default scope
    public static ServiceLifetime DefaultServiceLifetime { get; set; } = ServiceLifetime.Scoped;
    public static bool RegisterAsTypeWhenTypeHasInterfaces { get; set; } = false;
}