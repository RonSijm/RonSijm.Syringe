using RonSijm.Syringe.ExamplesB;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesB;

namespace RonSijm.Syringe.Tests.Features.Tests.MicrosoftServiceProvider.CustomCollection.Generics;

public class TestResolveGenericsWithoutRegistration_Exception : ResolveExamplesBDefaultsBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        var serviceCollection = new ServiceCollection();

        var fluent = serviceCollection.WireImplicit<ClassWith_FuncOfClassB>();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        return serviceProvider;
    }
}