using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public class PropertyInjectionServiceProviderFactory(IServiceProviderFactory<IServiceCollection> innerFactory) : IServiceProviderFactory<IServiceCollection>
{
    private readonly IServiceProviderFactory<IServiceCollection> _innerFactory = innerFactory ?? throw new ArgumentNullException(nameof(innerFactory));

    public IServiceCollection CreateBuilder(IServiceCollection services)
    {
        return _innerFactory.CreateBuilder(services);
    }

    public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
    {
        var syringeServiceProvider = new SyringeServiceProvider(containerBuilder, options => options.WithAfterGetServiceExtension(new PropertyInjectionAfterServiceExtension()));

        return syringeServiceProvider;
    }
}