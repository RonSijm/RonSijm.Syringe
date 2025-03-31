using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public class SyringeServiceProviderFactory(SyringeServiceProviderOptions options = null) : IServiceProviderFactory<SyringeServiceProviderBuilder>
{
    private readonly SyringeServiceProviderOptions _options = options ?? new SyringeServiceProviderOptions();

    public SyringeServiceProviderBuilder CreateBuilder(IServiceCollection services)
    {
        var container = new SyringeServiceProviderBuilder(services);
        return container;
    }

    public IServiceProvider CreateServiceProvider(SyringeServiceProviderBuilder builderOptions)
    {
        var serviceProvider = builderOptions.GetServiceProvider(_options);

        return serviceProvider;
    }
}