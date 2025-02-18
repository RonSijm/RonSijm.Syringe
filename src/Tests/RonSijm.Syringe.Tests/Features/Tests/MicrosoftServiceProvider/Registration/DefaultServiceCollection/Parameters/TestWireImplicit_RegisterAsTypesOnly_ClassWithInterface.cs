using RonSijm.Syringe.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Defaults;

namespace RonSijm.Syringe.Tests.Features.Tests.MicrosoftServiceProvider.Registration.DefaultServiceCollection.Parameters;

public class TestWireImplicit_RegisterAsTypesOnly_ClassWithInterface : ResolveExamplesANoExceptionsBase
{
    protected override Syringe.MicrosoftServiceProvider SetupServiceProvider()
    {
        #region CodeExample-Parameters-registerAsTypesOnly
        var serviceCollection = new ServiceCollection();
        serviceCollection.WireImplicit<ClassA>(registerAsTypesOnly: [typeof(ClassWithInterfaceA)]);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        #endregion

        return serviceProvider;
    }

    protected override void ClassWith_ClassWithInterface_AsInterfaceExpectations(Func<ClassWith_ClassWithInterface_AsInterfaceA> invocation)
    {
        invocation.CannotResolveServiceExpectation();
    }

    protected override void InterfaceFor_ClassWithInterfaceExpectations(Func<InterfaceFor_ClassWithInterfaceA> invocation)
    {
        invocation.NoRegistrationExpectation();
    }
}