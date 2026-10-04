namespace RonSijm.Syringe;

public abstract class SyringeServiceProviderAfterServiceExtensionBase : ISyringeServiceProviderAfterServiceExtension
{
    private readonly AsyncLocal<SyringeServiceProvider> _serviceProvider = new();
    protected SyringeServiceProvider ServiceProvider => _serviceProvider.Value;

    public void SetReference(SyringeServiceProvider serviceProvider)
    {
        _serviceProvider.Value = serviceProvider;
    }

    internal SyringeServiceProvider SwapReference(SyringeServiceProvider serviceProvider)
    {
        var previous = _serviceProvider.Value;
        _serviceProvider.Value = serviceProvider;
        return previous;
    }

    public abstract void Decorate(Type serviceType, object service);
}