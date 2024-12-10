using RonSijm.Syringe.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Defaults;

namespace RonSijm.Syringe.Tests.Features.Tests.CustomCollection.Parameters;

public class TestWireImplicit_DontRegisterTypesWithInterface_InterfaceFor_ClassWithInterface : ResolveExamplesANoExceptionsBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        #region CodeExample-DontRegisterTypesWithInterfaces
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.WireImplicit<Class>().DontRegisterTypesWithInterfaces([typeof(InterfaceFor_ClassWithInterface)]);
        #endregion

        var serviceProvider = serviceCollection.BuildServiceProvider();

        return serviceProvider;
    }

    protected override void ClassWith_ClassWithInterface_AsClassExpectations(Func<ClassWith_ClassWithInterface_AsClass> invocation)
    {
        invocation.UnableToResolveServiceExpectation(typeof(ClassWithInterface));
    }

    protected override void ClassWith_ClassWithInterface_AsInterfaceExpectations(Func<ClassWith_ClassWithInterface_AsInterface> invocation)
    {
        invocation.UnableToResolveServiceExpectation(typeof(InterfaceFor_ClassWithInterface));
    }

    protected override void InterfaceFor_ClassWithInterfaceExpectations(Func<InterfaceFor_ClassWithInterface> invocation)
    {
        invocation.NoRegistrationExpectation();
    }

    protected override void ClassWithInterfaceExpectations(Func<ClassWithInterface> invocation)
    {
        invocation.NoRegistrationExpectation();
    }
}