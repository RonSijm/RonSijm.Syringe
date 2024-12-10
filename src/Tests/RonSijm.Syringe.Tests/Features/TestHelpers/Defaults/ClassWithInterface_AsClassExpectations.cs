using RonSijm.Syringe.ExamplesA;

namespace RonSijm.Syringe.Tests.Features.TestHelpers.Defaults;

public static class ClassWithInterface_AsClassExpectations
{
    public static void CannotResolveServiceExpectation(this Func<ClassWith_ClassWithInterface_AsClass> invocation)
    {
        invocation.UnableToResolveServiceExpectation(typeof(ClassWithInterface));
    }
}