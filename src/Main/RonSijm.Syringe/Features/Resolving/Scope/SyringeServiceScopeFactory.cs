using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe.Scope;

public class SyringeServiceScopeFactory : IServiceScopeFactory
{
    private readonly SyringeServiceProvider _serviceProvider;

    public SyringeServiceScopeFactory(SyringeServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IServiceScope CreateScope()
    {
        return new SyringeServiceScope(_serviceProvider);
    }
}