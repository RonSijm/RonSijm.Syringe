using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe.Scope;

public class SyringeServiceScopeFactory : IServiceScopeFactory
{
    private readonly IServiceProvider _serviceProvider;

    public SyringeServiceScopeFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IServiceScope CreateScope()
    {
        return new SyringeServiceScope(_serviceProvider);
    }
}