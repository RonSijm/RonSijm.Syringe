using RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.BootStapping.SampleObjects;

namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.BootStapping;

public class SyringeServiceProvider_AddItemsTests
{
    [Fact]
    public async Task AddItemsAfterResolving()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<SimpleSampleObject1>();

        var serviceProvider = new Syringe.SyringeServiceProvider(serviceCollection);

        var sampleObject = serviceProvider.GetService<SimpleSampleObject1>();
        sampleObject.Should().NotBeNull();

        var sampleObject2 = serviceProvider.GetService<SimpleSampleObject2>();
        sampleObject2.Should().BeNull();

        var serviceCollection2 = new ServiceCollection();
        serviceCollection2.AddScoped<SimpleSampleObject2>();

        await serviceProvider.LoadServiceDescriptors(serviceCollection2);

        sampleObject2 = serviceProvider.GetService<SimpleSampleObject2>();
        sampleObject2.Should().BeNull(because: "We didn't rebuild the service provider yet.");

        serviceProvider.Build();

        sampleObject2 = serviceProvider.GetService<SimpleSampleObject2>();
        sampleObject2.Should().NotBeNull();
    }
}