namespace RonSijm.Syringe;
public class RegisterTypeAsSingletonAfterServiceExtension(Type type) : SyringeServiceProviderAfterServiceExtensionBase
{
    public override void Decorate(Type serviceType, object service)
    {
        if (service.GetType().IsAssignableTo(type))
        {
            ServiceProvider.Options.AdditionalProviders.Add(new SingletonProvider(type, service));
        }
    }
}