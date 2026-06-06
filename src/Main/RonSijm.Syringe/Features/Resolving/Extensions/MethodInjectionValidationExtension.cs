using System.Reflection;

namespace RonSijm.Syringe;

public class MethodInjectionValidationExtension : SyringeServiceProviderAfterServiceExtensionBase, IProvideCallSiteValidator
{
    private readonly List<MethodInfo> _methods = new();

    public MethodInjectionValidationExtension AddMethod(Delegate method)
    {
        _methods.Add(method.Method);
        return this;
    }

    public MethodInjectionValidationExtension AddMethod(MethodInfo method)
    {
        _methods.Add(method);
        return this;
    }

    public override void Decorate(Type serviceType, object service)
    {
    }

    public ISyringeCallSiteValidator CreateValidator(MicrosoftServiceProvider serviceProvider)
    {
        return new MethodInjectionCallSiteValidator(serviceProvider, _methods);
    }
}
