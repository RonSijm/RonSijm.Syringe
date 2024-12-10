using RonSijm.Syringe.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base;

namespace RonSijm.Syringe.Tests.Features.Tests.Registration.Attributes.LifetimeTests;

public class LifetimeTests_Transient : ServiceProviderSetupBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        #region CodeExample-TransientScope
        return typeof(ClassWithGuid).WireImplicit().WithLifetime(ServiceLifetime.Transient).BuildServiceProvider();
        #endregion
    }

    [Fact]
    public void Resolve_Class()
    {
        var service1 = ServiceProvider.GetRequiredService<ClassWithGuid>();
        var service2 = ServiceProvider.GetRequiredService<ClassWithGuid>();

        service1.Guid.Should().NotBe(service2.Guid);
    }
}