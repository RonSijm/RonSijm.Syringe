using RonSijm.Syringe.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Defaults;

namespace RonSijm.Syringe.Tests.Features.Tests.CustomCollection.Parameters;

public class TestWireImplicit_DontRegisterAsTypes_Class : ResolveExamplesADefaultsBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        #region CodeExample-DontRegisterTypesExtension
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.WireImplicit<Class>().DontRegisterTypes(typeof(Class));
        #endregion
        var serviceProvider = serviceCollection.BuildServiceProvider();

        return serviceProvider;
    }

    protected override void ClassExpectations(Func<Class> invocation)
    {
        invocation.NoRegistrationExpectation();
    }

    protected override void ClassWith_ClassExpectations(Func<ClassWith_Class> invocation)
    {
        invocation.UnableToResolveServiceExpectation(typeof(Class));
    }
}