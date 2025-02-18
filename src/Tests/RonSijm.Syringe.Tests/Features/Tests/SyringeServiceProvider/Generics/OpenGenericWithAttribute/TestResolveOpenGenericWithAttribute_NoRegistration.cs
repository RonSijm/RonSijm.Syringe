using RonSijm.Syringe.ExamplesB;
using RonSijm.Syringe.ExamplesB.OpenGeneric;
using RonSijm.Syringe.ExamplesB.OpenGenericWithAttribute;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base;

namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.Generics.OpenGenericWithAttribute;

public class TestResolveOpenGenericWithAttribute_NoRegistration : ServiceProviderSetupBase
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
        var service = ServiceProvider.GetRequiredService<IOpenGenericWithAttributeB<Class1B>>();
        service.Should().NotBeNull();
        service.GetType().Should().Be(typeof(IOpenGeneric2BClass2B));
    }

    [Fact]
    public void Resolve_Class2B_NoRegistration()
    {
        var service = ServiceProvider.GetRequiredService<IOpenGenericWithAttributeB<Class2B>>();
        service.Should().NotBeNull();
        service.GetType().Should().Be(typeof(IOpenGeneric2BDefaultImplementationB<Class2B>));
    }
}