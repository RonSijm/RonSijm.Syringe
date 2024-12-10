using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesA;

namespace RonSijm.Syringe.Tests.Features.Tests.ChainAssembliesTests;

public class TestWireImplicit_MultipleAssembliesExampleA : ResolveExamplesADefaultsBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        var serviceProvider = typeof(ExamplesA.Class).WireImplicit().WireImplicit<ExamplesB.Class>().BuildServiceProvider();

        return serviceProvider;
    }
}