namespace RonSijm.Syringe.ExamplesA;

[Registration.DontRegister]
public class ClassNoRegistrationAttributeA
{
    public Guid Guid { get; } = Guid.NewGuid();
}