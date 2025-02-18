using RonSijm.Syringe.ExamplesB;
using RonSijm.Syringe.ExamplesB.Helpers;
using RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesB;
using RonSijm.Syringe.Tests.Features.TestHelpers.Defaults;

namespace RonSijm.Syringe.Tests.Features.Tests.MicrosoftServiceProvider.CustomCollection.Generics;

public class TestResolveGenerics_OpenLazy : ResolveExamplesBNoExceptionsBase
{
    protected override IServiceProvider SetupServiceProvider()
    {
        var serviceCollection = new SyringeServiceCollection();

        serviceCollection.WireImplicit<ClassWith_FuncOfClassB>();
        serviceCollection.AddScoped(typeof(Lazy<>), typeof(Lazy<>));

        var serviceProvider = serviceCollection.BuildServiceProvider();

        return serviceProvider;
    }

    protected override void ClassWith_FuncOfClassExpectations(Func<ClassWith_FuncOfClassB> invocation)
    {
        invocation.UnableToResolveServiceExpectation($"System.Func`1[{ExamplesBConstants.ExpectedNamespace}.{nameof(Class1B)}]");
    }
}