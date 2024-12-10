using RonSijm.Syringe.ExamplesB;
using RonSijm.Syringe.ExamplesB.Helpers;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesB;
using RonSijm.Syringe.Tests.Features.TestHelpers.Defaults;

namespace RonSijm.Syringe.Tests.Features.Tests.CustomCollection.Generics;

public class TestResolveGenerics_OpenFunc : ResolveExamplesBNoExceptionsBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.WireImplicit<Class>().AddScopedFunc<Class>();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        return serviceProvider;
    }

    protected override void ClassWith_LazyOfClassExpectations(Func<ClassWith_LazyOfClass> invocation)
    {
        invocation.UnableToResolveServiceExpectation($"System.Lazy`1[{ExamplesBConstants.ExpectedNamespace}.{nameof(Class)}]");
    }
}