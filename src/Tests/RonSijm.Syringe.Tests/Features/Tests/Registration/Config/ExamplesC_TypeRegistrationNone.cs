using RonSijm.Syringe.ExamplesC.MultipleClassesOnInterface;
using RonSijm.Syringe.Tests.Features.TestHelpers;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base;

namespace RonSijm.Syringe.Tests.Features.Tests.Registration.Config;

public class ExamplesC_TypeRegistrationNone : ServiceProviderSetupBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        #region CodeExample-ConfigurationNotSpecificClass

        var appsetting = """
                         {
                             "DependencyInjection": {
                                 "Assembly:RonSijm.Syringe.ExamplesC": {
                                 "Type:ExampleC2": "None",
                                 "Type:ExampleC3": "None",
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
        service1.Count.Should().Be(1);
        service1[0].GetType().Name.Should().Be(nameof(ExampleC1));
    }
}