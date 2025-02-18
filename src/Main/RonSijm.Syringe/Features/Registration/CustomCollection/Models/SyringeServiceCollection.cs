using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public partial class SyringeServiceCollection(IServiceCollection services) : IHaveServiceCollection
{
    public IServiceCollection InnerServiceCollection { get; } = services;
    internal List<IWireSyringeServiceDescriptorExtension> Extensions { get; set; } = [];

    internal readonly Queue<SyringeServiceDescriptor> Descriptors = new();

    public SyringeServiceCollection() : this(new ServiceCollection())
    {
    }

    public IServiceProvider BuildServiceProvider()
    {
        BuildServiceCollection();
        return InnerServiceCollection.BuildServiceProvider();
    }

    public SyringeServiceProvider BuildSyringeServiceProvider(Action<SyringeServiceProviderOptions> options = null)
    {
        var collection = BuildServiceCollection();
        var provider = new SyringeServiceProvider(collection, options);
        return provider;
    }

    public IServiceCollection BuildServiceCollection()
    {
        // When you build the service provider, all the pending registrations get dequeued and added to the inner collection.
        // They get dequeued because you cannot modify them anymore after building.
        while (Descriptors.Count != 0)
        {
            var registration = Descriptors.Dequeue();

            Extensions.ForEach(x => x.BeforeBuildServiceProvider(registration));

            InnerServiceCollection.WireImplicit(registration.Assembly, registration.ServiceLifetime, registration.RegistrationSettings);
        }

        return InnerServiceCollection;
    }
}