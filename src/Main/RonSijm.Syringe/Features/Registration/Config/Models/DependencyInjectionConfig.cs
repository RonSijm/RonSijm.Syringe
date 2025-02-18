namespace RonSijm.Syringe;

public class DependencyInjectionConfig
{
    public Dictionary<string, AssemblyConfig> Assembly { get; set; } = new();
}

public class AssemblyConfig
{
    public Dictionary<string, RegistrationType> Type { get; set; } = new();
}