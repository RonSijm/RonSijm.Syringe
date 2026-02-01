namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.PrimitiveInjection;

public class ClassWithIntConstructor(int intParam)
{
    public int Value { get; private set; } = intParam;
}

