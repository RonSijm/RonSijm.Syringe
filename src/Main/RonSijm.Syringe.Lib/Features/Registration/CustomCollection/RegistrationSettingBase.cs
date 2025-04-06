namespace RonSijm.Syringe;

public abstract class RegistrationSettingBase
{
    public RegistrationSettingBase()
    {
        
    }

    public RegistrationSettingBase(RegistrationType registrationType)
    {
        RegistrationType = registrationType;
    }

    public RegistrationType RegistrationType { get; set; }
}