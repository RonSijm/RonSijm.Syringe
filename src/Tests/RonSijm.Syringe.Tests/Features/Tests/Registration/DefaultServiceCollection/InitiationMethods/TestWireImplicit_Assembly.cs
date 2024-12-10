using RonSijm.Syringe.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesA;

namespace RonSijm.Syringe.Tests.Features.Tests.Registration.DefaultServiceCollection.InitiationMethods;

public class TestWireImplicit_Assembly : ResolveExamplesADefaultsBase
{
    protected override MicrosoftServiceProvider SetupServiceProvider()
    {
        #region CodeExample-DefaultServiceCollection-WireByAssembly
        var serviceCollection = new ServiceCollection();
        var assembly = typeof(Class).Assembly;
        serviceCollection.WireImplicit(assembly, ServiceLifetime.Transient, []);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        #endregion

        return serviceProvider;
    }
}