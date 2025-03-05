namespace RonSijm.Syringe;

public abstract class AdditionByCriteriaProvider : AdditionProvider
{
    protected Func<Type, bool> Criteria { get; set; }

    public override bool IsMatch(Type serviceType)
    {
        return Criteria(serviceType);
    }
}