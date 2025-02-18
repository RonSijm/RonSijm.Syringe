using RonSijm.Syringe.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesA;

namespace RonSijm.Syringe.Tests.Features.Tests.MicrosoftServiceProvider.Registration.DefaultServiceCollection.InitiationMethods;

public class TestWireImplicit_Type : ResolveExamplesADefaultsBase
{
    protected override Syringe.MicrosoftServiceProvider SetupServiceProvider()
    {
        #region CodeExample-DefaultServiceCollection-WireByType

        var serviceCollection = new ServiceCollection();
        serviceCollection.WireImplicit(typeof(ClassA));
        var serviceProvider = serviceCollection.BuildServiceProvider();

        #endregion

        return serviceProvider;
    }
}