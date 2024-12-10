namespace RonSijm.Syringe.Tests.Features.TestHelpers.Base;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class ServiceProviderSetupBase
{
    protected ServiceProviderSetupBase()
    {
        _serviceProviderFactory = new Lazy<IServiceProvider>(SetupServiceProvider);
    }

    protected abstract IServiceProvider SetupServiceProvider();

    private readonly Lazy<IServiceProvider> _serviceProviderFactory;

    protected IServiceProvider ServiceProvider => _serviceProviderFactory.Value;
}