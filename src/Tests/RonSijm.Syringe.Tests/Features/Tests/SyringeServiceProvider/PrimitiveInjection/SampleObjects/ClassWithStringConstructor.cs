namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.PrimitiveInjection;

public class ClassWithStringConstructor(string constructorParam)
{
    public string Value { get; private set; } = constructorParam;
}