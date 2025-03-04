namespace RonSijm.Syringe;

public class OptionalProvider() : AdditionOpenGenericProvider(typeof(Optional<>))
{
    public override object Create(Type serviceType, SyringeServiceProvider serviceProvider)
    {
        var innerType = serviceType.GetGenericArguments()[0];
        var optional = OpenGenericType.MakeGenericType(innerType);
        dynamic wrapper = Activator.CreateInstance(optional);

        var valueForInnerType = serviceProvider.GetService(innerType);

        if (valueForInnerType == null)
        {
            return wrapper;
        }

        wrapper?.SetValue(valueForInnerType);
        return wrapper;
    }
}