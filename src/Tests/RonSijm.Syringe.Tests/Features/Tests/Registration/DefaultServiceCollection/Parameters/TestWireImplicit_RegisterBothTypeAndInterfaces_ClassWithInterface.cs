using RonSijm.Syringe.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesA;

namespace RonSijm.Syringe.Tests.Features.Tests.Registration.DefaultServiceCollection.Parameters;

public class TestWireImplicit_RegisterBothTypeAndInterfaces_ClassWithInterface : ResolveExamplesANoExceptionsBase
{
    protected override MicrosoftServiceProvider SetupServiceProvider()
    {
        #region CodeExample-Parameters-registerBothTypeAndInterfaces
        var serviceCollection = new ServiceCollection();
        serviceCollection.WireImplicit<Class>(registerBothTypeAndInterfaces: [typeof(ClassWithInterface)]);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        #endregion

        return serviceProvider;
    }
}