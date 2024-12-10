using RonSijm.Syringe.Attributes;

namespace RonSijm.Syringe.ExamplesA;

[Registration.DontRegister]
public class ClassNoRegistrationAttribute
{
    public Guid Guid { get; } = Guid.NewGuid();
}