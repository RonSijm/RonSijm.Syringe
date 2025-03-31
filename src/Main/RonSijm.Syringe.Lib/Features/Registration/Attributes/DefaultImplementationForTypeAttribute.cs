namespace RonSijm.Syringe;

public class DefaultImplementationForTypeAttribute(Type type) : Attribute
{
    public Type Type { get; init; } = type;
}