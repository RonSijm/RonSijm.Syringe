namespace RonSijm.Syringe.Tests.Features.TestHelpers;

public class InjectAttribute : Attribute
{
    public bool Required { get; set; }
    public object Key { get; set; }
}