using RonSijm.Syringe.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesA;

namespace RonSijm.Syringe.Tests.Features.Tests.CustomCollection.InitiationMethods;

public class TestWireImplicit_TypeExtension : ResolveExamplesADefaultsBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        var serviceProvider = typeof(Class).WireImplicit().BuildServiceProvider();

        return serviceProvider;
    }
}