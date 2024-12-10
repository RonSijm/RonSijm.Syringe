using RonSijm.Syringe.ExamplesB;
using RonSijm.Syringe.ExamplesB.Helpers;
using RonSijm.Syringe.Tests.Features.TestHelpers.Defaults;

namespace RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesB;

public abstract class ResolveExamplesBDefaultsBase : ResolveExamplesBNoExceptionsBase
{
    protected override void ClassWith_FuncOfClassExpectations(Func<ClassWith_FuncOfClass> invocation)
    {
        invocation.UnableToResolveServiceExpectation($"System.Func`1[{ExamplesBConstants.ExpectedNamespace}.{nameof(Class)}]");
    }

    protected override void ClassWith_LazyOfClassExpectations(Func<ClassWith_LazyOfClass> invocation)
    {
        invocation.UnableToResolveServiceExpectation($"System.Lazy`1[{ExamplesBConstants.ExpectedNamespace}.{nameof(Class)}]");
    }
}