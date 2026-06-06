namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.PropertyResolving;

public class TestResolveRequiredProperties
{
    [Fact]
    public void RequiredProperty_IsResolved_WhenServiceRegistered()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ClassWithRequiredProperty>();
        serviceCollection.AddSingleton<ChildClassWithProperty>();

        var serviceProvider = serviceCollection.BuildSyringeServiceProvider(options => options.WithAfterGetService(new PropertyInjectionAfterServiceExtension()));

        var result = serviceProvider.GetService<ClassWithRequiredProperty>();

        result.Should().NotBeNull();
        result.RequiredChild.Should().NotBeNull();
    }

    [Fact]
    public void RequiredProperty_Throws_WhenServiceNotRegistered()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ClassWithRequiredProperty>();

        var serviceProvider = serviceCollection.BuildSyringeServiceProvider(options => options.WithAfterGetService(new PropertyInjectionAfterServiceExtension()));

        var invocation = serviceProvider.Invoking(sp => sp.GetService<ClassWithRequiredProperty>());

        invocation.Should().Throw<InvalidOperationException>()
            .WithMessage($"Unable to resolve required property 'RequiredChild' of type '{typeof(ChildClassWithProperty)}' on '{typeof(ClassWithRequiredProperty).FullName}'.");
    }

    [Fact]
    public void OptionalProperty_RemainsNull_WhenServiceNotRegistered()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ClassWithOptionalProperty>();

        var serviceProvider = serviceCollection.BuildSyringeServiceProvider(options => options.WithAfterGetService(new PropertyInjectionAfterServiceExtension()));

        var result = serviceProvider.GetService<ClassWithOptionalProperty>();

        result.Should().NotBeNull();
        result.OptionalChild.Should().BeNull();
    }


    [Fact]
    public void RequiredProperty_Throws_WhenServiceNotRegisteredAndValidateIsCalled()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ClassWithRequiredProperty>();

        var invocation = FluentActions.Invoking(() => serviceCollection.BuildSyringeServiceProvider(options =>
        {
            options.WithAfterGetService(new PropertyInjectionAfterServiceExtension());
            options.ValidateOnBuild = true;
        }));

        invocation.Should().Throw<AggregateException>()
            .WithMessage("Some services are not able to be constructed*")
            .WithInnerException<InvalidOperationException>()
            .WithMessage($"*Unable to resolve required property 'RequiredChild' of type '{typeof(ChildClassWithProperty)}' on '{typeof(ClassWithRequiredProperty).FullName}'.*");
    }

    [Fact]
    public void RequiredProperty_DoesNotThrow_WhenServiceRegisteredAndValidateIsCalled()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ClassWithRequiredProperty>();
        serviceCollection.AddSingleton<ChildClassWithProperty>();

        var invocation = FluentActions.Invoking(() => serviceCollection.BuildSyringeServiceProvider(options =>
        {
            options.WithAfterGetService(new PropertyInjectionAfterServiceExtension());
            options.ValidateOnBuild = true;
        }));

        invocation.Should().NotThrow();
    }
}
