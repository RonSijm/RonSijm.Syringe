using Fluxor;
using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public static class ServiceProviderServiceExtensions
{
    public static IState<T> GetState<T>(this IServiceProvider provider)
    {
        return provider.GetService<IState<T>>();
    }

    public static async Task<IState<T>> WaitUntil<T>(this IState<T> provider, Func<T, bool> condition)
    {
        for (var i = 0; i < 100; i++)
        {
            var isExpected = condition(provider.Value);
            if (isExpected)
            {
                return provider;
            }

            await Task.Delay(100);
        }

        throw new TimeoutException("Took too long to reach expected state.");
    }
}