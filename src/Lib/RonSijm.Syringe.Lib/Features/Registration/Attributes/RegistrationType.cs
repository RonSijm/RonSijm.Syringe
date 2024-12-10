namespace RonSijm.Syringe.Attributes;

[Flags]
public enum RegistrationType
{
    None = 0,

    Type = 1 << 0,
    Interface = 1 << 1,

    TypeAndInterface = Type | Interface,
}