namespace RonSijm.Syringe.ExamplesB.OpenGenericWithAttribute;

[DefaultImplementationForType(typeof(IOpenGenericWithAttributeB<>))]
public class IOpenGeneric2BDefaultImplementationB<T> : IOpenGenericWithAttributeB<T>
{
    public List<T> Values { get; set; }
}