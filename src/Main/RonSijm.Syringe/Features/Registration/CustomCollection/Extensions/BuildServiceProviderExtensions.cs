namespace RonSijm.Syringe;
public static class BuildServiceProviderExtensions
{
    public static IServiceProvider BuildServiceProvider(this SyringeServiceCollection collection)
    {
        collection.BuildServiceCollection();
        return collection.InnerServiceCollection.BuildServiceProvider();
    }

    public static SyringeServiceProvider BuildSyringeServiceProvider(this SyringeServiceCollection collection, Action<SyringeServiceProviderOptions> options = null)
    {
        var buildServiceCollection = collection.BuildServiceCollection();
        var provider = new SyringeServiceProvider(buildServiceCollection, options);
        return provider;
    }

    public static SyringeServiceProvider BuildSyringeServiceProvider(this SyringeServiceCollectionAndRegistration collection, Action<SyringeServiceProviderOptions> options = null)
    {
        return collection.Collection.BuildSyringeServiceProvider(options);
    }

    public static SyringeServiceProvider BuildServiceProvider(this SyringeServiceCollectionAndRegistration collection, Action<SyringeServiceProviderOptions> options = null)
    {
        return collection.Collection.BuildSyringeServiceProvider(options);
    }
}