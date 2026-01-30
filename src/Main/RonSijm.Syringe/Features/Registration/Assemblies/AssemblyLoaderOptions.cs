namespace RonSijm.Syringe;

/// <summary>
/// Options for assembly loading behavior
/// </summary>
public class AssemblyLoaderOptions
{
    /// <summary>
    /// Disables cascade loading of referenced assemblies
    /// </summary>
    public bool DisableCascadeLoading { get; set; }

    /// <summary>
    /// Enables logging for cascade loading errors
    /// </summary>
    public bool EnableLoggingForCascadeErrors { get; set; }

    /// <summary>
    /// Extensions that are called after assemblies are loaded
    /// </summary>
    public List<ILoadAfterExtension> AfterLoadAssembliesExtensions { get; set; } = [];

    /// <summary>
    /// Enables general logging for assembly loading
    /// </summary>
    public bool EnableLogging { get; set; }
}
