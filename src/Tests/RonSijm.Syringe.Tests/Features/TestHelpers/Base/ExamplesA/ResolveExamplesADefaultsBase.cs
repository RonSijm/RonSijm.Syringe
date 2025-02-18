using RonSijm.Syringe.ExamplesA;
using RonSijm.Syringe.Tests.Features.TestHelpers.Defaults;

namespace RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesA;

public abstract class ResolveExamplesADefaultsBase : ResolveExamplesANoExceptionsBase
{
    protected override void ClassWithInterfaceExpectations(Func<ClassWithInterfaceA> invocation)
    {
        invocation.NoRegistrationExpectation();
    }

    protected override void ClassWith_ClassWithInterface_AsClassExpectations(Func<ClassWith_ClassWithInterface_AsClassA> invocation)
    {
        invocation.CannotResolveServiceExpectation();
    }
}