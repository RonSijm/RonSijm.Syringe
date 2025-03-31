namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.Scopes;

public class TestResolveScoped
{
    [Fact]
    public void CanResolveRecursivePropertiesTest()
    {
        var serviceProvider = SetupServiceProvider();
        var rootResult1 = serviceProvider.GetService<ClassWithGuid>();
        var rootResult2 = serviceProvider.GetService<ClassWithGuid>();

        var scope1 = serviceProvider.CreateScope();
        var scopedResult1_BeforeNewScope = scope1.ServiceProvider.GetService<ClassWithGuid>();

        var scope2 = serviceProvider.CreateScope();
        var scopedResult2 = scope2.ServiceProvider.GetService<ClassWithGuid>();
        var scopedResult1_AfterNewScope = scope1.ServiceProvider.GetService<ClassWithGuid>();

        rootResult1.Id.Should().Be(rootResult2.Id);
        rootResult1.Id.Should().NotBe(scopedResult1_BeforeNewScope.Id);
        rootResult1.Id.Should().NotBe(scopedResult2.Id);
        scopedResult1_BeforeNewScope.Id.Should().NotBe(scopedResult2.Id);
        scopedResult1_BeforeNewScope.Id.Should().Be(scopedResult1_AfterNewScope.Id);
    }

    protected IServiceProvider SetupServiceProvider()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddScoped<ClassWithGuid>();

        var serviceProvider = serviceCollection.BuildSyringeServiceProvider();

        return serviceProvider;
    }

    protected IServiceProvider SetupNativeServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<ClassWithGuid>();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        return serviceProvider;
    }
}