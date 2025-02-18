using RonSijm.Syringe.ExamplesC.MultipleClassesOnInterface;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base;

namespace RonSijm.Syringe.Tests.Features.Tests.MicrosoftServiceProvider.Registration.Config;

public class ExamplesC_NoConfigTests : ServiceProviderSetupBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        return new SyringeServiceCollection()
            .WireImplicit<ExampleCInterface>().BuildServiceProvider();
    }

    [Fact]
    public void Resolve_Interface()
    {
        var service1 = ServiceProvider.GetRequiredService<IEnumerable<ExampleCInterface>>().ToList();
        service1.Count.Should().Be(3);
    }
}