using RonSijm.Syringe.ExamplesA;

namespace RonSijm.Syringe.Tests.Features.TestHelpers.Defaults;

public static class ClassWith_ClassWithInterface_AsInterfaceExpectations
{
    public static void CannotResolveServiceExpectation(this Func<ClassWith_ClassWithInterface_AsInterfaceA> invocation)
    {
        invocation.UnableToResolveServiceExpectation(typeof(InterfaceFor_ClassWithInterfaceA));
    }
}