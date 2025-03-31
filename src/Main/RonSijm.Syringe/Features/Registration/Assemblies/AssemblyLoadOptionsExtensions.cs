// ReSharper disable global UnusedMember.Global
// ReSharper disable global UnusedType.Global

namespace RonSijm.Syringe;

public static class AssemblyLoadOptionsExtensions
{
    public static SettingsForAssembly UseCustomClass(this SettingsForAssembly options, string classPath)
    {
        options.assembly.ClassPath = classPath;
        return options;
    }

    public static SettingsForAssembly UseCustomRelativePath(this SettingsForAssembly options, string relativePath)
    {
        options.assembly.RelativePath = relativePath;
        return options;
    }

    public static SettingsForAssembly UseOptions(this SettingsForAssembly options, Action<AssemblyOptions> optionsConfig)
    {
        optionsConfig(options.assembly);
        return options;
    }

    public static SettingsForAssembly DisableCascadeLoading(this SettingsForAssembly options)
    {
        options.assembly.DisableCascadeLoading = true;
        return options;
    }

    public static SettingsForAssembly UseHttpHandler(this SettingsForAssembly options, Func<string, HttpRequestMessage, AssemblyOptions, bool> handler)
    {
        options.assembly.HttpHandler = handler;
        return options;
    }
}