namespace RonSijm.Syringe;

public interface ISyringeServiceProviderAfterServiceExtension : ISyringeExtension
{
    void Decorate(Type serviceType, object service);
}