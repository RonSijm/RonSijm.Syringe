namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.PrimitiveInjection;

public class ClassWithMultiplePrimitives(string stringParam, int intParam, bool boolParam)
{
    public string StringValue { get; private set; } = stringParam;
    public int IntValue { get; private set; } = intParam;
    public bool BoolValue { get; private set; } = boolParam;
}

