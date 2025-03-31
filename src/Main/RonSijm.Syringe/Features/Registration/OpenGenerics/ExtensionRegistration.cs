namespace RonSijm.Syringe;

// ReSharper disable once UnusedType.Global
public static class ExtensionRegistration
{
    public static void RegisterOptional(this List<AdditionProvider> additionalProviders)
    {
        var optionalProvider = new OptionalProvider();
        additionalProviders.Add(optionalProvider);
    }

    public static void RegisterLazy(this List<AdditionProvider> additionalProviders)
    {
        var lazyResolveProvider = new LazyResolveProvider();
        additionalProviders.Add(lazyResolveProvider);
    }
}