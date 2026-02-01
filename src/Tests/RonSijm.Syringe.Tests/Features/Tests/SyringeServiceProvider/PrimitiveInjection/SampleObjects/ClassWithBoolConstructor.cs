namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.PrimitiveInjection;

public class ClassWithBoolConstructor(bool boolParam)
{
    public bool Value { get; private set; } = boolParam;
}

