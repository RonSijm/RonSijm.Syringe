namespace RonSijm.Syringe;

public static class OpenGenericServiceRegistration
{
    public static AdditionProvider RegisterOpenGeneric(Type openGenericType, Func<Type, IServiceProvider, object> functionForInnerType)
    {
        var result = RegisterOpenGeneric(openGenericType, (_, childType, serviceProvider) => functionForInnerType.Invoke(childType, serviceProvider));
        return result;
    }

    public static AdditionProvider RegisterOpenGeneric(Type openGenericType, Func<Type, Type, IServiceProvider, object> functionForInnerType)
    {
        var additionOpenGenericFactory = new AdditionOpenGenericProvider(openGenericType, functionForInnerType);
        return additionOpenGenericFactory;
    }
}