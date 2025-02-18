namespace RonSijm.Syringe.ExamplesB.OpenGenericWithAttribute;

public interface IOpenGenericWithAttributeB<T>
{
    public List<T> Values { get; set; }
}