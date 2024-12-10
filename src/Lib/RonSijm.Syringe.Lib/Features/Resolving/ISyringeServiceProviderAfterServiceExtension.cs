namespace RonSijm.Syringe;

public interface ISyringeServiceProviderAfterServiceExtension
{
    void SetReference(SyringeServiceProvider serviceProvider);
    void Decorate(object service);
}