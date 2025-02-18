using RonSijm.Syringe.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base;

namespace RonSijm.Syringe.Tests.Features.Tests.MicrosoftServiceProvider.Registration.Attributes.LifetimeTests;

public class LifetimeTests_Transient : ServiceProviderSetupBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        #region CodeExample-TransientScope
        return typeof(ClassWithGuidA).WireImplicit().WithLifetime(ServiceLifetime.Transient).BuildServiceProvider();
        #endregion
    }

    [Fact]
    public void Resolve_Class()
    {
        var service1 = ServiceProvider.GetRequiredService<ClassWithGuidA>();
        var service2 = ServiceProvider.GetRequiredService<ClassWithGuidA>();

        service1.Guid.Should().NotBe(service2.Guid);
    }
}