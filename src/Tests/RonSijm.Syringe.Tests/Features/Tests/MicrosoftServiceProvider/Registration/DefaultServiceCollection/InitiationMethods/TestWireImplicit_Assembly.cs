using RonSijm.Syringe.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesA;

namespace RonSijm.Syringe.Tests.Features.Tests.MicrosoftServiceProvider.Registration.DefaultServiceCollection.InitiationMethods;

public class TestWireImplicit_Assembly : ResolveExamplesADefaultsBase
{
    protected override Syringe.MicrosoftServiceProvider SetupServiceProvider()
    {
        #region CodeExample-DefaultServiceCollection-WireByAssembly
        var serviceCollection = new ServiceCollection();
        var assembly = typeof(ClassA).Assembly;
        serviceCollection.WireImplicit(assembly, ServiceLifetime.Transient, []);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        #endregion

        return serviceProvider;
    }
}