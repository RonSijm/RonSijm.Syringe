using RonSijm.Syringe.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Defaults;

namespace RonSijm.Syringe.Tests.Features.Tests.MicrosoftServiceProvider.CustomCollection.Parameters;

public class TestWireImplicit_DontRegisterTypesWithInterface_InterfaceFor_ClassWithInterface : ResolveExamplesANoExceptionsBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        #region CodeExample-DontRegisterTypesWithInterfaces
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.WireImplicit<ClassA>().DontRegisterTypesWithInterfaces(typeof(InterfaceFor_ClassWithInterfaceA));
        #endregion

        var serviceProvider = serviceCollection.BuildServiceProvider();

        return serviceProvider;
    }

    protected override void ClassWith_ClassWithInterface_AsClassExpectations(Func<ClassWith_ClassWithInterface_AsClassA> invocation)
    {
        invocation.UnableToResolveServiceExpectation(typeof(ClassWithInterfaceA));
    }

    protected override void ClassWith_ClassWithInterface_AsInterfaceExpectations(Func<ClassWith_ClassWithInterface_AsInterfaceA> invocation)
    {
        invocation.UnableToResolveServiceExpectation(typeof(InterfaceFor_ClassWithInterfaceA));
    }

    protected override void InterfaceFor_ClassWithInterfaceExpectations(Func<InterfaceFor_ClassWithInterfaceA> invocation)
    {
        invocation.NoRegistrationExpectation();
    }

    protected override void ClassWithInterfaceExpectations(Func<ClassWithInterfaceA> invocation)
    {
        invocation.NoRegistrationExpectation();
    }
}