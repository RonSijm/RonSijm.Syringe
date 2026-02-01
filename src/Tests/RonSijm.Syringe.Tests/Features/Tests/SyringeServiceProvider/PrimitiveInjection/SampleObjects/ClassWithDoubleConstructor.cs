namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.PrimitiveInjection;

public class ClassWithDoubleConstructor(double doubleParam)
{
    public double Value { get; private set; } = doubleParam;
}

