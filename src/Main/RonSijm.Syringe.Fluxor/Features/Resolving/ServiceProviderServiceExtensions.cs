using Fluxor;
using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public static class ServiceProviderServiceExtensions
{
    public static IState<T> GetState<T>(this IServiceProvider provider)
    {
        return provider.GetService<IState<T>>();
    }
}