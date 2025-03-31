namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.PropertyResolving;

public class TestResolveRecursiveProperties
{
    [Fact]
    public void CanResolveRecursivePropertiesTest()
    {
        var serviceProvider = SetupServiceProvider();
        var result = serviceProvider.GetService<ClassWithProperty>();
    }

    protected IServiceProvider SetupServiceProvider()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ClassWithProperty>();
        serviceCollection.AddSingleton<ChildClassWithProperty>();

        var serviceProvider = serviceCollection.BuildSyringeServiceProvider(options => options.WithAfterGetService(new PropertyInjectionAfterServiceExtension()));

        return serviceProvider;
    }
}