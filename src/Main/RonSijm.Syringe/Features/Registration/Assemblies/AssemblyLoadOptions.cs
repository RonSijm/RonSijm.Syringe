namespace RonSijm.Syringe;
public class AssemblyLoadOptions
{
    private List<(Func<string, bool> Criteria, AssemblyOptions Options)> _assemblyMapping;

    public AssemblyOptions GetOptions(string assemblyName)
    {
        if (_assemblyMapping == null)
        {
            return null;
        }

        if (assemblyName.EndsWith(".wasm", StringComparison.Ordinal))
        {
            assemblyName = assemblyName.Replace(".wasm", string.Empty);
        }

        foreach (var mapping in _assemblyMapping)
        {
            if (mapping.Criteria(assemblyName))
            {
                return mapping.Options;
            }
        }

        return null;
    }

    public SettingsForAssembly UseSettingsForDll(string assemblyPath)
    {
        return UseSettingsWhen(x => x == assemblyPath);
    }

    public SettingsForAssembly UseSettingsWhen(Func<string, bool> criteria)
    {
        _assemblyMapping ??= [];

        var assemblyOptions = new AssemblyOptions();
        _assemblyMapping.Add((criteria, assemblyOptions));

        return (this, assemblyOptions);
    }

    public void Add(Func<string, bool> criteria, AssemblyOptions assemblyOptions)
    {
        _assemblyMapping ??= [];
        _assemblyMapping.Add((criteria, assemblyOptions));
    }
}