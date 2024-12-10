using RonSijm.Syringe.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Defaults;

namespace RonSijm.Syringe.Tests.Features.Tests.CustomCollection.Parameters;

public class TestWireImplicit_RegisterAsTypesOnly_ClassWithInterface : ResolveExamplesANoExceptionsBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        #region CodeExample-RegisterAsTypesOnly
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.WireImplicit<Class>().RegisterAsTypesOnly([typeof(ClassWithInterface)]);
        #endregion
        var serviceProvider = serviceCollection.BuildServiceProvider();

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