using RonSijm.Syringe.ExamplesB;
using RonSijm.Syringe.ExamplesB.OpenGeneric;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base;

namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.Generics.OpenGeneric;

public class TestResolveOpenGeneric_NoRegistration : ServiceProviderSetupBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.WireImplicit<Class1B>();

        var serviceProvider = serviceCollection.BuildSyringeServiceProvider();

        return serviceProvider;
    }

    [Fact]
    public void Resolve_Class1B_NoRegistration()
    {
        var service = ServiceProvider.GetRequiredService<IOpenGeneric1B<Class1B>>();
        service.Should().NotBeNull();
        service.GetType().Should().Be(typeof(IOpenGeneric1BClass1B));
    }

    [Fact]
    public void Resolve_Class2B_NoRegistration()
    {
        var service = ServiceProvider.GetRequiredService<IOpenGeneric1B<Class2B>>();
        service.Should().Be(null);
    }
}