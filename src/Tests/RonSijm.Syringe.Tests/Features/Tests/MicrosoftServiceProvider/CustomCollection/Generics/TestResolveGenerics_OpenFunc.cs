using RonSijm.Syringe.ExamplesB;
using RonSijm.Syringe.ExamplesB.Helpers;
using RonSijm.Syringe.Helpers;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesB;
using RonSijm.Syringe.Tests.Features.TestHelpers.Defaults;

namespace RonSijm.Syringe.Tests.Features.Tests.MicrosoftServiceProvider.CustomCollection.Generics;

public class TestResolveGenerics_OpenFunc : ResolveExamplesBNoExceptionsBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.WireImplicit<Class1B>().AddScopedFunc<Class1B>();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        return serviceProvider;
    }

    protected override void ClassWith_LazyOfClassExpectations(Func<ClassWith_LazyOfClassB> invocation)
    {
        invocation.UnableToResolveServiceExpectation($"System.Lazy`1[{ExamplesBConstants.ExpectedNamespace}.{nameof(Class1B)}]");
    }
}