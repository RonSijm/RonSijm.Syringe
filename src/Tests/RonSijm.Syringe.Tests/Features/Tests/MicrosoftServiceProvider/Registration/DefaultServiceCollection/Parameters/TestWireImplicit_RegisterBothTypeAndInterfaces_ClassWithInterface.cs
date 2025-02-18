using RonSijm.Syringe.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesA;

namespace RonSijm.Syringe.Tests.Features.Tests.MicrosoftServiceProvider.Registration.DefaultServiceCollection.Parameters;

public class TestWireImplicit_RegisterBothTypeAndInterfaces_ClassWithInterface : ResolveExamplesANoExceptionsBase
{
    protected override Syringe.MicrosoftServiceProvider SetupServiceProvider()
    {
        #region CodeExample-Parameters-registerBothTypeAndInterfaces
        var serviceCollection = new ServiceCollection();
        serviceCollection.WireImplicit<ClassA>(registerBothTypeAndInterfaces: [typeof(ClassWithInterfaceA)]);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        #endregion

        return serviceProvider;
    }
}