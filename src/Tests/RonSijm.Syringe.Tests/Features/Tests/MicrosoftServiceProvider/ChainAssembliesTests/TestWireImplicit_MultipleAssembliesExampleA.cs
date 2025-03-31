using RonSijm.Syringe.ExamplesA;
using RonSijm.Syringe.ExamplesB;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesA;

namespace RonSijm.Syringe.Tests.Features.Tests.MicrosoftServiceProvider.ChainAssembliesTests;

public class TestWireImplicit_MultipleAssembliesExampleA : ResolveExamplesADefaultsBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        SyringeServiceCollectionAndRegistration? syringeServiceCollectionAndRegistration = typeof(ClassA).WireImplicit().WireImplicit<Class1B>();
        syringeServiceCollectionAndRegistration.BuildServiceProvider();
        var serviceProvider = syringeServiceCollectionAndRegistration.BuildServiceProvider();

        return serviceProvider;
    }
}