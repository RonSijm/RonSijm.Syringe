using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesA;

namespace RonSijm.Syringe.Tests.Features.Tests.MicrosoftServiceProvider.ChainAssembliesTests;

public class TestWireImplicit_MultipleAssembliesExampleA : ResolveExamplesADefaultsBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        var serviceProvider = typeof(ExamplesA.ClassA).WireImplicit().WireImplicit<ExamplesB.Class1B>().BuildServiceProvider();

        return serviceProvider;
    }
}