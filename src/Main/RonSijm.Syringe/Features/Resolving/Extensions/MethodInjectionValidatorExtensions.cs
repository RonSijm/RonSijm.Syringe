using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

/// <summary>
/// Validates that the <c>[FromServices]</c> / <c>[FromKeyedServices]</c> parameters of one or more
/// static (or instance) methods are resolvable from a given <see cref="IServiceProvider"/>.
/// Works against any provider that exposes the standard <see cref="IServiceProviderIsService"/>
/// (and optionally <see cref="IServiceProviderIsKeyedService"/>) introspection seams, including
/// the default Microsoft DI container.
/// </summary>
public static class MethodInjectionValidatorExtensions
{
    public static void ValidateMethodInjection(this IServiceProvider provider, params Delegate[] methods)
    {
        provider.ValidateMethodInjection(methods.Select(m => m.Method).ToArray());
    }

    public static void ValidateMethodInjection(this IServiceProvider provider, params MethodInfo[] methods)
    {
        var isService = provider.GetService<IServiceProviderIsService>()
                        ?? throw new InvalidOperationException(
                            $"The provided '{provider.GetType().FullName}' does not expose '{nameof(IServiceProviderIsService)}'. " +
                            "Method injection validation requires a provider that supports service introspection.");
        var isKeyedService = provider.GetService<IServiceProviderIsKeyedService>();

        var exceptions = new List<Exception>();

        foreach (var method in methods)
        {
            foreach (var (parameter, _, key) in MethodInjectionAttributeScanner.GetInjectableParameters(method))
            {
                bool isRegistered;

                if (key != null)
                {
                    if (isKeyedService == null)
                    {
                        exceptions.Add(new InvalidOperationException(
                            $"The provided '{provider.GetType().FullName}' does not expose '{nameof(IServiceProviderIsKeyedService)}', " +
                            $"cannot validate keyed parameter '{parameter.Name}' on method '{method.DeclaringType?.FullName}.{method.Name}'."));
                        continue;
                    }

                    isRegistered = isKeyedService.IsKeyedService(parameter.ParameterType, key);
                }
                else
                {
                    isRegistered = isService.IsService(parameter.ParameterType);
                }

                if (isRegistered)
                {
                    continue;
                }

                exceptions.Add(new InvalidOperationException(
                    MethodInjectionAttributeScanner.FormatMissingParameter(method, parameter, key)));
            }
        }

        if (exceptions.Count > 0)
        {
            throw new AggregateException("Some services are not able to be constructed", exceptions);
        }
    }
}
