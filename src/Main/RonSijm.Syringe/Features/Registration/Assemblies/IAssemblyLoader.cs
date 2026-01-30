using System.Reflection;

namespace RonSijm.Syringe;

/// <summary>
/// Generic interface for assembly loading functionality.
/// This interface provides the core assembly loading capabilities without any framework-specific dependencies.
/// </summary>
public interface IAssemblyLoader
{
    /// <summary>
    /// Loads a single assembly by name
    /// </summary>
    /// <param name="assemblyToLoad">The name of the assembly to load (e.g., "MyAssembly.wasm")</param>
    /// <returns>A list of loaded assemblies (may include cascade-loaded dependencies)</returns>
    Task<List<Assembly>> LoadAssemblyAsync(string assemblyToLoad);

    /// <summary>
    /// Loads multiple assemblies by name
    /// </summary>
    /// <param name="assembliesToLoad">The names of the assemblies to load</param>
    /// <returns>A list of loaded assemblies (may include cascade-loaded dependencies)</returns>
    Task<List<Assembly>> LoadAssembliesAsync(IEnumerable<string> assembliesToLoad);

    /// <summary>
    /// Gets the list of assemblies that have been loaded by this loader
    /// </summary>
    List<Assembly> AdditionalAssemblies { get; }
}
