using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesB;

namespace RonSijm.Syringe.Tests.Features.Tests.ChainAssembliesTests;

public class TestWireImplicit_MultipleAssembliesExampleB : ResolveExamplesBDefaultsBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        var serviceProvider = typeof(ExamplesA.Class).WireImplicit().WireImplicit<ExamplesB.Class>().BuildServiceProvider();

        return serviceProvider;
    }
}