using RonSijm.Syringe.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesA;

namespace RonSijm.Syringe.Tests.Features.Tests.Registration.DefaultServiceCollection.InitiationMethods;

public class TestWireImplicit_Type : ResolveExamplesADefaultsBase
{
    protected override MicrosoftServiceProvider SetupServiceProvider()
    {
        #region CodeExample-DefaultServiceCollection-WireByType

        var serviceCollection = new ServiceCollection();
        serviceCollection.WireImplicit(typeof(Class));
        var serviceProvider = serviceCollection.BuildServiceProvider();

        #endregion

        return serviceProvider;
    }
}