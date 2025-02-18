using RonSijm.Syringe.ExamplesB;
using RonSijm.Syringe.ExamplesB.OpenGeneric;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base;

namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.Generics.OpenGeneric;

public class TestResolveOpenGeneric_OpenRegistration : ServiceProviderSetupBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton(typeof(IOpenGeneric1B<>), typeof(IOpenGeneric1BDefaultImplementationB<>));

        var serviceProvider = serviceCollection.BuildSyringeServiceProvider();

        return serviceProvider;
    }

    [Fact]
    public void Resolve_Class2B_NoRegistration()
    {
        var service = ServiceProvider.GetRequiredService<IOpenGeneric1B<Class2B>>();
        service.Should().NotBeNull();
        service.GetType().Should().Be(typeof(IOpenGeneric1BDefaultImplementationB<Class2B>));
    }
}