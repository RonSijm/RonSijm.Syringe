using RonSijm.Syringe.ExamplesC.MultipleClassesOnInterface;
using RonSijm.Syringe.Tests.Features.TestHelpers;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base;

namespace RonSijm.Syringe.Tests.Features.Tests.MicrosoftServiceProvider.Registration.Config;

public class ExamplesC_InterfaceRegistrationNone : ServiceProviderSetupBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        #region CodeExample-ConfigurationNotWithInterfaces

        var appsetting = """
                         {
                             "DependencyInjection": {
                                 "Assembly:RonSijm.Syringe.ExamplesC": {
                                 "Type:ExampleCInterface": "None"
                                 }
                             }
                         }
                         """.ToConfiguration();

        #endregion

        return new SyringeServiceCollection()
            .WithConfig(appsetting)
            .WireImplicit<ExampleCInterface>().BuildServiceProvider();
    }

    [Fact]
    public void Resolve_Interface()
    {
        var service1 = ServiceProvider.GetRequiredService<IEnumerable<ExampleCInterface>>().ToList();
        service1.Count.Should().Be(0);
    }
}