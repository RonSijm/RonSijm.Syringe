using System.Reflection;
using RonSijm.Syringe.ServiceLookup;

namespace RonSijm.Syringe;

internal sealed class MethodInjectionCallSiteValidator(MicrosoftServiceProvider serviceProvider, IReadOnlyList<MethodInfo> methods) : ISyringeCallSiteValidator
{
    private bool _hasValidated;

    public void ValidateCallSite(ServiceCallSite callSite)
    {
        if (_hasValidated)
        {
            return;
        }

        _hasValidated = true;

        var errors = new List<string>();

        // ReSharper disable once LoopCanBeConvertedToQuery - Justification: Creates incomprehensible garbage linq.
        foreach (var method in methods)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery - Justification: Creates incomprehensible garbage linq.
            foreach (var (parameter, _, key) in MethodInjectionAttributeScanner.GetInjectableParameters(method))
            {
                var isRegistered = key != null
                    ? serviceProvider.CallSiteFactory.IsKeyedService(parameter.ParameterType, key)
                    : serviceProvider.CallSiteFactory.IsService(parameter.ParameterType);

                if (isRegistered)
                {
                    continue;
                }

                errors.Add(MethodInjectionAttributeScanner.FormatMissingParameter(method, parameter, key));
            }
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
        }
    }
}
