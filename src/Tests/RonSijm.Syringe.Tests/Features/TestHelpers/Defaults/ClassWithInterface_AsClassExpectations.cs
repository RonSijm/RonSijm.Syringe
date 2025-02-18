using RonSijm.Syringe.ExamplesA;

namespace RonSijm.Syringe.Tests.Features.TestHelpers.Defaults;

public static class ClassWithInterface_AsClassExpectations
{
    public static void CannotResolveServiceExpectation(this Func<ClassWith_ClassWithInterface_AsClassA> invocation)
    {
        invocation.UnableToResolveServiceExpectation(typeof(ClassWithInterfaceA));
    }
}