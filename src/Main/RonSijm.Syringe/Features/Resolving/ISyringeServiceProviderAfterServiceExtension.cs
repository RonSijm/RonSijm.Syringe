namespace RonSijm.Syringe;

public interface ISyringeServiceProviderAfterServiceExtension : ISyringeExtension
{
    void Decorate(object service);
}