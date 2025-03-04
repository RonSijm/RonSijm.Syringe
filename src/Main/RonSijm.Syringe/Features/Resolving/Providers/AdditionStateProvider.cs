namespace RonSijm.Syringe;

public class AdditionStateProvider : AdditionProvider
{
    public Func<Type, IServiceProvider, object> StateFactory { get; set; }

    public override object Create(Type serviceType, SyringeServiceProvider serviceProvider)
    {
        return StateFactory(serviceType, serviceProvider);
    }
}