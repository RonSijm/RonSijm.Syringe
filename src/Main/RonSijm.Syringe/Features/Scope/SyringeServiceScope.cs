using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe.Scope;

public class SyringeServiceScope : IServiceScope
{
    public SyringeServiceScope(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public IServiceProvider ServiceProvider { get; }
}