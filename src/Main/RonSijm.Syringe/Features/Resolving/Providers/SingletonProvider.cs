namespace RonSijm.Syringe;

public class SingletonProvider(Type descriptorServiceType, object instance) : AdditionProvider
{
    public override bool IsMatch(Type serviceType)
    {
        return descriptorServiceType == serviceType;
    }

    public override object Create(Type serviceType, SyringeServiceProvider serviceProvider)
    {
        return instance;
    }
}