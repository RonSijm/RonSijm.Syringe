namespace RonSijm.Syringe.Attributes;

public abstract class Registration
{
    public class DontRegisterAttribute() : RegistrationAttribute(RegistrationType.None);

    public class RegistrationAttribute(RegistrationType registration) : Attribute
    {
        public RegistrationType Registration { get; } = registration;
    }
}