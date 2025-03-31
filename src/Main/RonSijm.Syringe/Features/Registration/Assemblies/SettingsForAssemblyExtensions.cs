namespace RonSijm.Syringe;
public static class SettingsForAssemblyExtensions
{
    public static SettingsForAssembly UseSettingsForDll(this SyringeServiceProviderOptions options, string assemblyPath)
    {
        return UseSettingsWhen(options, x => x == assemblyPath);
    }

    public static SettingsForAssembly UseSettingsWhen(this SyringeServiceProviderOptions options, Func<string, bool> criteria)
    {
        var assemblyLoadOptions = GetAssemblyLoadOptions(options);
        var assemblyOptions = new AssemblyOptions();
        assemblyLoadOptions.Add(criteria, assemblyOptions);

        return (assemblyLoadOptions, assemblyOptions);
    }

    public static AssemblyLoadOptions GetAssemblyLoadOptions(this SyringeServiceProviderOptions options)
    {
        if (options.ExtendedOptions.FirstOrDefault(x => x is AssemblyLoadOptions) is AssemblyLoadOptions assemblyLoadOptions)
        {
            return assemblyLoadOptions;
        }

        assemblyLoadOptions = new AssemblyLoadOptions();
        options.ExtendedOptions.Add(assemblyLoadOptions);

        return assemblyLoadOptions;
    }

    public static AssemblyOptions GetAssemblyOptions(this SyringeServiceProviderOptions options, string assemblyName)
    {
        var assemblyLoadOptions = options.ExtendedOptions.FirstOrDefault(x => x is AssemblyLoadOptions) as AssemblyLoadOptions;
        return assemblyLoadOptions?.GetOptions(assemblyName);
    }
}
