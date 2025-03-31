using System.Reflection;

namespace RonSijm.Syringe;

public class LazyResolveProvider() : AdditionOpenGenericProvider(typeof(Lazy<>))
{
    public override object Create(Type serviceType, SyringeServiceProvider serviceProvider)
    {
        var innerType = serviceType.GetGenericArguments()[0];

        var lazyType = OpenGenericType.MakeGenericType(innerType);

        var factory = typeof(LazyResolveProvider)
            .GetMethod(nameof(CreateLazy), BindingFlags.NonPublic | BindingFlags.Static)
            .MakeGenericMethod(innerType);

        return factory.Invoke(null, [serviceProvider]);
    }

    private static Lazy<T> CreateLazy<T>(SyringeServiceProvider serviceProvider)
    {
        return new Lazy<T>(() => (T)serviceProvider.GetService(typeof(T)));
    }
}