namespace RonSijm.Syringe;

public abstract class SyringeServiceProviderAfterServiceExtensionBase : ISyringeServiceProviderAfterServiceExtension
{
    protected SyringeServiceProvider ServiceProvider { get; private set; }

    public void SetReference(SyringeServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public abstract void Decorate(Type serviceType, object service);
}