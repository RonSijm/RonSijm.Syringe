using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public class SyringeServiceProviderBuilder(IServiceCollection services)
{
    public IServiceProvider GetServiceProvider(SyringeServiceProviderOptions options)
    {
        options.AdditionalProviders.RegisterOptional();
        options.AdditionalProviders.RegisterLazy();

        var serviceProvider = new SyringeServiceProvider(services, options);
        return serviceProvider;
    }
}