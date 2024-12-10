using RonSijm.Syringe.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base;

namespace RonSijm.Syringe.Tests.Features.Tests.Registration.Attributes.LifetimeTests;

public class LifetimeTests_Scoped : ServiceProviderSetupBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        #region CodeExample-DefaultScope
        return typeof(ClassWithGuid).WireImplicit().BuildServiceProvider();
        #endregion
    }

    [Fact]
    public void Resolve_Class()
    {
        var service1 = ServiceProvider.GetRequiredService<ClassWithGuid>();
        var service2 = ServiceProvider.GetRequiredService<ClassWithGuid>();

        service1.Guid.Should().Be(service2.Guid);
    }

    [Fact]
    public void Resolve_Class_With_Scope()
    {
        var service1 = ServiceProvider.CreateScope().ServiceProvider.GetRequiredService<ClassWithGuid>();
        var service2 = ServiceProvider.CreateScope().ServiceProvider.GetRequiredService<ClassWithGuid>();

        service1.Guid.Should().NotBe(service2.Guid);
    }
}