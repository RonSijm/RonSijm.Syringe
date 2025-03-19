namespace RonSijm.Syringe;

public class AdditionOpenGenericProvider : AdditionByCriteriaProvider
{
    protected Type OpenGenericType { get; set; }
    protected Func<Type, Type, IServiceProvider, object> FunctionForInnerType { get; set; }

    public AdditionOpenGenericProvider(Type openGenericType, Func<Type, Type, IServiceProvider, object> functionForInnerType)
    {
        OpenGenericType = openGenericType;
        FunctionForInnerType = functionForInnerType;

        Criteria = serviceType => serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == OpenGenericType;
    }

    protected AdditionOpenGenericProvider(Type openGenericType)
    {
        OpenGenericType = openGenericType;

        Criteria = serviceType => serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == OpenGenericType;
    }

    public override object Create(Type serviceType, SyringeServiceProvider serviceProvider)
    {
        var innerType = serviceType.GetGenericArguments()[0];
        var result = FunctionForInnerType.Invoke(OpenGenericType, innerType, serviceProvider);

        return result;
    }
}