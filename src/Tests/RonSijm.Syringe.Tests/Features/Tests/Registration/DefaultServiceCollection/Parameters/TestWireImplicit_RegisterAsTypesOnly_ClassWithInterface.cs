using RonSijm.Syringe.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Defaults;

namespace RonSijm.Syringe.Tests.Features.Tests.Registration.DefaultServiceCollection.Parameters;

public class TestWireImplicit_RegisterAsTypesOnly_ClassWithInterface : ResolveExamplesANoExceptionsBase
{
    protected override MicrosoftServiceProvider SetupServiceProvider()
    {
        #region CodeExample-Parameters-registerAsTypesOnly
        var serviceCollection = new ServiceCollection();
        serviceCollection.WireImplicit<Class>(registerAsTypesOnly: [typeof(ClassWithInterface)]);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        #endregion

        return serviceProvider;
    }

    protected override void ClassWith_ClassWithInterface_AsInterfaceExpectations(Func<ClassWith_ClassWithInterface_AsInterface> invocation)
    {
        invocation.CannotResolveServiceExpectation();
    }

    protected override void InterfaceFor_ClassWithInterfaceExpectations(Func<InterfaceFor_ClassWithInterface> invocation)
    {
        invocation.NoRegistrationExpectation();
    }
}