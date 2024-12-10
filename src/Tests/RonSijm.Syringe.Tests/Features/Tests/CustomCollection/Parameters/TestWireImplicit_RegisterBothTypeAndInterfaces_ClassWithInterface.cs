using RonSijm.Syringe.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesA;

namespace RonSijm.Syringe.Tests.Features.Tests.CustomCollection.Parameters;

public class TestWireImplicit_RegisterBothTypeAndInterfaces_ClassWithInterface : ResolveExamplesANoExceptionsBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        #region CodeExample-RegisterBothTypeAndInterfaces
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.WireImplicit<Class>().RegisterBothTypeAndInterfaces([typeof(ClassWithInterface)]);
        #endregion
        var serviceProvider = serviceCollection.BuildServiceProvider();

        return serviceProvider;
    }
}