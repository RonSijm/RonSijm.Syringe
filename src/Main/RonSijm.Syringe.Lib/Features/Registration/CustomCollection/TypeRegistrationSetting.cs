namespace RonSijm.Syringe;

public class TypeRegistrationSetting : RegistrationSettingBase
{
    public TypeRegistrationSetting()
    {
        
    }

    public TypeRegistrationSetting(Type type, RegistrationType registrationType) : base(registrationType)
    {
        Type = type;
    }

    public Type Type { get; set; }
}