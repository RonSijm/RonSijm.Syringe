using RonSijm.Syringe.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesA;

namespace RonSijm.Syringe.Tests.Features.Tests.MicrosoftServiceProvider.Registration.DefaultServiceCollection.InitiationMethods;

public class TestWireImplicit_GenericType : ResolveExamplesADefaultsBase
{
    protected override Syringe.MicrosoftServiceProvider SetupServiceProvider()
    {
        #region CodeExample-DefaultServiceCollection-WireByTypeGeneric

        var serviceCollection = new ServiceCollection();
        serviceCollection.WireImplicit<ClassA>();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        #endregion

        return serviceProvider;
    }
}