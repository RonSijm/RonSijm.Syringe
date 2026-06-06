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

    [Fact]
    public void RequiredKeyedProperty_IsResolved_WhenServiceRegisteredWithCorrectKey()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ClassWithRequiredKeyedProperty>();
        serviceCollection.AddKeyedSingleton<ChildClassWithProperty>(ClassWithRequiredKeyedProperty.ExpectedKey);

        var serviceProvider = serviceCollection.BuildSyringeServiceProvider(options => options.WithAfterGetService(new PropertyInjectionAfterServiceExtension()));

        var result = serviceProvider.GetService<ClassWithRequiredKeyedProperty>();

        result.Should().NotBeNull();
        result.KeyedChild.Should().NotBeNull();
    }

    [Fact]
    public void RequiredKeyedProperty_Throws_WhenServiceRegisteredWithoutKey()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ClassWithRequiredKeyedProperty>();
        serviceCollection.AddSingleton<ChildClassWithProperty>();

        var serviceProvider = serviceCollection.BuildSyringeServiceProvider(options => options.WithAfterGetService(new PropertyInjectionAfterServiceExtension()));

        var invocation = serviceProvider.Invoking(sp => sp.GetService<ClassWithRequiredKeyedProperty>());

        invocation.Should().Throw<InvalidOperationException>()
            .WithMessage($"Unable to resolve required property 'KeyedChild' of type '{typeof(ChildClassWithProperty)}' with key '{ClassWithRequiredKeyedProperty.ExpectedKey}' on '{typeof(ClassWithRequiredKeyedProperty).FullName}'.");
    }

    [Fact]
    public void RequiredKeyedProperty_Throws_WhenServiceRegisteredWithWrongKey()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ClassWithRequiredKeyedProperty>();
        serviceCollection.AddKeyedSingleton<ChildClassWithProperty>("SomeOtherKey");

        var serviceProvider = serviceCollection.BuildSyringeServiceProvider(options => options.WithAfterGetService(new PropertyInjectionAfterServiceExtension()));

        var invocation = serviceProvider.Invoking(sp => sp.GetService<ClassWithRequiredKeyedProperty>());

        invocation.Should().Throw<InvalidOperationException>()
            .WithMessage($"Unable to resolve required property 'KeyedChild' of type '{typeof(ChildClassWithProperty)}' with key '{ClassWithRequiredKeyedProperty.ExpectedKey}' on '{typeof(ClassWithRequiredKeyedProperty).FullName}'.");
    }

    [Fact]
    public void RequiredKeyedProperty_Throws_WhenServiceRegisteredWithoutKeyAndValidateIsCalled()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ClassWithRequiredKeyedProperty>();
        serviceCollection.AddSingleton<ChildClassWithProperty>();

        var invocation = FluentActions.Invoking(() => serviceCollection.BuildSyringeServiceProvider(options =>
        {
            options.WithAfterGetService(new PropertyInjectionAfterServiceExtension());
            options.ValidateOnBuild = true;
        }));

        invocation.Should().Throw<AggregateException>()
            .WithMessage("Some services are not able to be constructed*")
            .WithInnerException<InvalidOperationException>()
            .WithMessage($"*Unable to resolve required property 'KeyedChild' of type '{typeof(ChildClassWithProperty)}' with key '{ClassWithRequiredKeyedProperty.ExpectedKey}' on '{typeof(ClassWithRequiredKeyedProperty).FullName}'.*");
    }

    [Fact]
    public void RequiredKeyedProperty_Throws_WhenServiceRegisteredWithWrongKeyAndValidateIsCalled()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ClassWithRequiredKeyedProperty>();
        serviceCollection.AddKeyedSingleton<ChildClassWithProperty>("SomeOtherKey");

        var invocation = FluentActions.Invoking(() => serviceCollection.BuildSyringeServiceProvider(options =>
        {
            options.WithAfterGetService(new PropertyInjectionAfterServiceExtension());
            options.ValidateOnBuild = true;
        }));

        invocation.Should().Throw<AggregateException>()
            .WithMessage("Some services are not able to be constructed*")
            .WithInnerException<InvalidOperationException>()
            .WithMessage($"*Unable to resolve required property 'KeyedChild' of type '{typeof(ChildClassWithProperty)}' with key '{ClassWithRequiredKeyedProperty.ExpectedKey}' on '{typeof(ClassWithRequiredKeyedProperty).FullName}'.*");
    }

    [Fact]
    public void RequiredKeyedProperty_DoesNotThrow_WhenServiceRegisteredWithCorrectKeyAndValidateIsCalled()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ClassWithRequiredKeyedProperty>();
        serviceCollection.AddKeyedSingleton<ChildClassWithProperty>(ClassWithRequiredKeyedProperty.ExpectedKey);

        var invocation = FluentActions.Invoking(() => serviceCollection.BuildSyringeServiceProvider(options =>
        {
            options.WithAfterGetService(new PropertyInjectionAfterServiceExtension());
            options.ValidateOnBuild = true;
        }));

        invocation.Should().NotThrow();
    }

    [Fact]
    public void RequiredEnumKeyedProperty_IsResolved_WhenServiceRegisteredWithCorrectKey()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ClassWithRequiredEnumKeyedProperty>();
        serviceCollection.AddKeyedSingleton<ChildClassWithProperty>(ClassWithRequiredEnumKeyedProperty.ExpectedKey);

        var serviceProvider = serviceCollection.BuildSyringeServiceProvider(options => options.WithAfterGetService(new PropertyInjectionAfterServiceExtension()));

        var result = serviceProvider.GetService<ClassWithRequiredEnumKeyedProperty>();

        result.Should().NotBeNull();
        result.EnumKeyedChild.Should().NotBeNull();
    }

    [Fact]
    public void RequiredEnumKeyedProperty_DoesNotThrow_WhenServiceRegisteredWithCorrectKeyAndValidateIsCalled()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ClassWithRequiredEnumKeyedProperty>();
        serviceCollection.AddKeyedSingleton<ChildClassWithProperty>(ClassWithRequiredEnumKeyedProperty.ExpectedKey);

        var invocation = FluentActions.Invoking(() => serviceCollection.BuildSyringeServiceProvider(options =>
        {
            options.WithAfterGetService(new PropertyInjectionAfterServiceExtension());
            options.ValidateOnBuild = true;
        }));

        invocation.Should().NotThrow();
    }

    [Fact]
    public void RequiredEnumKeyedProperty_Throws_WhenServiceRegisteredWithWrongKeyAndValidateIsCalled()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ClassWithRequiredEnumKeyedProperty>();
        serviceCollection.AddKeyedSingleton<ChildClassWithProperty>(TestServiceKey.Beta);

        var invocation = FluentActions.Invoking(() => serviceCollection.BuildSyringeServiceProvider(options =>
        {
            options.WithAfterGetService(new PropertyInjectionAfterServiceExtension());
            options.ValidateOnBuild = true;
        }));

        invocation.Should().Throw<AggregateException>()
            .WithMessage("Some services are not able to be constructed*")
            .WithInnerException<InvalidOperationException>()
            .WithMessage($"*Unable to resolve required property 'EnumKeyedChild' of type '{typeof(ChildClassWithProperty)}' with key '{ClassWithRequiredEnumKeyedProperty.ExpectedKey}' on '{typeof(ClassWithRequiredEnumKeyedProperty).FullName}'.*");
    }
}
