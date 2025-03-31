using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe.Scope;

public class SyringeServiceScope : IServiceScope, IServiceProvider
{
    private readonly SyringeServiceProvider _root;
    private readonly ScopeWrapper _scopeWrapper;

    public SyringeServiceScope(SyringeServiceProvider serviceProvider)
    {
        _scopeWrapper = serviceProvider.CreateScope();
        _root = serviceProvider;
    }

    public void Dispose()
    {
        _root.DisposeScope(_scopeWrapper);
        GC.SuppressFinalize(this);
    }

    public IServiceProvider ServiceProvider => this;

    public object GetService(Type serviceType)
    {
        return _scopeWrapper.ServiceProvider.GetService(serviceType);
    }
}