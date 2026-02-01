namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.PrimitiveInjection;

public class ClassWithGuidConstructor(Guid guidParam)
{
    public Guid Value { get; private set; } = guidParam;
}

