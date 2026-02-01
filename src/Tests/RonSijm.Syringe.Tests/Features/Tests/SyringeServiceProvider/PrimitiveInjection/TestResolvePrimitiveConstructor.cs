namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.PrimitiveInjection;

public class TestResolvePrimitiveConstructor
{
    [Fact]
    public void CanResolveStringPrimitiveByParameterName()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ClassWithStringConstructor>();
        serviceCollection.AddKeyedSingleton<string>("constructorParam", "test");

        var serviceProvider = serviceCollection.BuildSyringeServiceProvider();
        var result = serviceProvider.GetRequiredService<ClassWithStringConstructor>();

        result.Value.Should().Be("test");
    }

    [Fact]
    public void CanResolveIntPrimitiveByParameterName()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ClassWithIntConstructor>();
        serviceCollection.AddKeyedSingleton("intParam", 42);

        var serviceProvider = serviceCollection.BuildSyringeServiceProvider();
        var result = serviceProvider.GetRequiredService<ClassWithIntConstructor>();

        result.Value.Should().Be(42);
    }

    [Fact]
    public void CanResolveBoolPrimitiveByParameterName()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ClassWithBoolConstructor>();
        serviceCollection.AddKeyedSingleton("boolParam", true);

        var serviceProvider = serviceCollection.BuildSyringeServiceProvider();
        var result = serviceProvider.GetRequiredService<ClassWithBoolConstructor>();

        result.Value.Should().BeTrue();
    }

    [Fact]
    public void CanResolveDoublePrimitiveByParameterName()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ClassWithDoubleConstructor>();
        serviceCollection.AddKeyedSingleton("doubleParam", 3.14159);

        var serviceProvider = serviceCollection.BuildSyringeServiceProvider();
        var result = serviceProvider.GetRequiredService<ClassWithDoubleConstructor>();

        result.Value.Should().BeApproximately(3.14159, 0.00001);
    }

    [Fact]
    public void CanResolveGuidPrimitiveByParameterName()
    {
        var expectedGuid = Guid.NewGuid();

        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ClassWithGuidConstructor>();
        serviceCollection.AddKeyedSingleton("guidParam", expectedGuid);

        var serviceProvider = serviceCollection.BuildSyringeServiceProvider();
        var result = serviceProvider.GetRequiredService<ClassWithGuidConstructor>();

        result.Value.Should().Be(expectedGuid);
    }

    [Fact]
    public void CanResolveMultiplePrimitivesInSameConstructor()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ClassWithMultiplePrimitives>();
        serviceCollection.AddKeyedSingleton<string>("stringParam", "hello");
        serviceCollection.AddKeyedSingleton("intParam", 123);
        serviceCollection.AddKeyedSingleton("boolParam", true);

        var serviceProvider = serviceCollection.BuildSyringeServiceProvider();
        var result = serviceProvider.GetRequiredService<ClassWithMultiplePrimitives>();

        result.StringValue.Should().Be("hello");
        result.IntValue.Should().Be(123);
        result.BoolValue.Should().BeTrue();
    }

    [Fact]
    public void CanResolveMixedServiceAndPrimitiveParameters()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<ISimpleService, SimpleService>();
        serviceCollection.AddSingleton<ClassWithMixedConstructor>();
        serviceCollection.AddKeyedSingleton<string>("connectionString", "Server=localhost;Database=test");
        serviceCollection.AddKeyedSingleton("maxRetries", 5);

        var serviceProvider = serviceCollection.BuildSyringeServiceProvider();
        var result = serviceProvider.GetRequiredService<ClassWithMixedConstructor>();

        result.Service.Should().NotBeNull();
        result.Service.GetValue().Should().Be("SimpleServiceValue");
        result.ConnectionString.Should().Be("Server=localhost;Database=test");
        result.MaxRetries.Should().Be(5);
    }

    [Fact]
    public void PrimitiveInjectionWorksWithTransientLifetime()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddTransient<ClassWithIntConstructor>();
        serviceCollection.AddKeyedSingleton("intParam", 99);

        var serviceProvider = serviceCollection.BuildSyringeServiceProvider();
        var result1 = serviceProvider.GetRequiredService<ClassWithIntConstructor>();
        var result2 = serviceProvider.GetRequiredService<ClassWithIntConstructor>();

        result1.Value.Should().Be(99);
        result2.Value.Should().Be(99);
        result1.Should().NotBeSameAs(result2); // Transient creates new instances
    }

    [Fact]
    public void PrimitiveInjectionWorksWithScopedLifetime()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddScoped<ClassWithStringConstructor>();
        serviceCollection.AddKeyedSingleton<string>("constructorParam", "scoped-test");

        var serviceProvider = serviceCollection.BuildSyringeServiceProvider();

        var scope = serviceProvider.CreateScope();
        var result1 = scope.ServiceProvider.GetRequiredService<ClassWithStringConstructor>();
        var result2 = scope.ServiceProvider.GetRequiredService<ClassWithStringConstructor>();

        result1.Value.Should().Be("scoped-test");
        result1.Should().BeSameAs(result2); // Same instance within scope
    }
}