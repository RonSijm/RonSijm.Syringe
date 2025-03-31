using RonSijm.Syringe.Tests.TestObjects.SampleObjects;

namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.BootStapping;

public class SyringeServiceProvider_AddTestLibTests
{
    [Fact]
    public async Task AddAssemblyWithMissingDependencies()
    {
        var serviceProvider = new Syringe.SyringeServiceProvider();

        var weatherResolver = serviceProvider.GetService<ISimpleObjectInLibraryWithInterface>();
        weatherResolver.Should().BeNull(because: "We haven't registered anything yet.");

        await serviceProvider.RegisterAssembly(typeof(SimpleObjectInLibraryWithInterface).Assembly);

        weatherResolver = serviceProvider.GetService<ISimpleObjectInLibraryWithInterface>();
        weatherResolver.Should().BeNull(because: "We didn't rebuild the service provider yet.");

        serviceProvider.Build();

        Action action = () => serviceProvider.GetService<ISimpleObjectInLibraryWithInterface>();
        action.Should().Throw<InvalidOperationException>(because: "It requires an HttpClient that we didn't register");
    }

    [Fact]
    public async Task AddAssemblyWithImplicitBootstrap()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<HttpClient>();
        var serviceProvider = new Syringe.SyringeServiceProvider(serviceCollection);

        var weatherResolver = serviceProvider.GetService<ISimpleObjectInLibraryWithInterface>();
        weatherResolver.Should().BeNull(because: "We haven't registered anything yet.");

        await serviceProvider.RegisterAssemblies(typeof(SimpleObjectInLibraryWithInterface).Assembly);

        weatherResolver = serviceProvider.GetService<ISimpleObjectInLibraryWithInterface>();
        weatherResolver.Should().BeNull(because: "We didn't rebuild the service provider yet.");

        serviceProvider.Build();

        weatherResolver = serviceProvider.GetService<ISimpleObjectInLibraryWithInterface>();
        weatherResolver.Should().NotBeNull();
    }
}