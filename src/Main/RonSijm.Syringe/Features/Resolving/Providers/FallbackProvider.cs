namespace RonSijm.Syringe;
public class FallbackProvider : AdditionProvider
{
    public Func<Type, bool> CanHandleType { get; set; }
    public Func<Type, object> GetService { get; set; }

    public override bool IsMatch(Type serviceType)
    {
        var exists = CanHandleType(serviceType);
        return exists;
    }

    public override object Create(Type serviceType, SyringeServiceProvider serviceProvider)
    {
        var service = GetService(serviceType);
        return service;
    }
}