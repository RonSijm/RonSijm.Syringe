namespace RonSijm.Syringe;

// ReSharper disable once UnusedType.Global
public static class OptionalRegistration
{
    public static void RegisterOptional(this List<AdditionProvider> additionalProviders)
    {
        var optionalProvider = new OptionalProvider();
        additionalProviders.Add(optionalProvider);
    }
}