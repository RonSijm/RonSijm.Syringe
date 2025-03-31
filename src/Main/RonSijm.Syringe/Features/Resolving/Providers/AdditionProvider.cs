namespace RonSijm.Syringe;

public abstract class AdditionProvider
{
    public abstract bool IsMatch(Type serviceType);

    public abstract object Create(Type serviceType, SyringeServiceProvider serviceProvider);
}