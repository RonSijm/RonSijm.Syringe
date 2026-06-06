namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.MethodInjection;

public class TestValidateMethodInjection
{
    [Fact]
    public void MethodInjection_DoesNotThrow_WhenServiceRegistered()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddSingleton<IGetFootballService, GetFootballService>();

        var extension = new MethodInjectionValidationExtension()
            .AddMethod(FootballEndpoints.GetFootballAsync);

        var invocation = FluentActions.Invoking(() => serviceCollection.BuildSyringeServiceProvider(options =>
        {
            options.WithAfterGetService(extension);
            options.ValidateOnBuild = true;
        }));

        invocation.Should().NotThrow();
    }

    [Fact]
    public void MethodInjection_Throws_WhenServiceNotRegistered()
    {
        var serviceCollection = new SyringeServiceCollection();

        var extension = new MethodInjectionValidationExtension()
            .AddMethod(FootballEndpoints.GetFootballAsync);

        var invocation = FluentActions.Invoking(() => serviceCollection.BuildSyringeServiceProvider(options =>
        {
            options.WithAfterGetService(extension);
            options.ValidateOnBuild = true;
        }));

        invocation.Should().Throw<AggregateException>()
            .WithMessage("Some services are not able to be constructed*")
            .WithInnerException<InvalidOperationException>()
            .WithMessage($"*Unable to resolve required parameter 'service' of type '{typeof(IGetFootballService)}' on method '{typeof(FootballEndpoints).FullName}.{nameof(FootballEndpoints.GetFootballAsync)}'.*");
    }

    [Fact]
    public void MethodInjection_DoesNotThrow_WhenKeyedServiceRegisteredWithCorrectKey()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddKeyedSingleton<IGetFootballService, GetFootballService>(FootballKey.Primary);

        var extension = new MethodInjectionValidationExtension()
            .AddMethod(FootballEndpoints.GetKeyedFootballAsync);

        var invocation = FluentActions.Invoking(() => serviceCollection.BuildSyringeServiceProvider(options =>
        {
            options.WithAfterGetService(extension);
            options.ValidateOnBuild = true;
        }));

        invocation.Should().NotThrow();
    }

    [Fact]
    public void MethodInjection_Throws_WhenKeyedServiceRegisteredWithWrongKey()
    {
        var serviceCollection = new SyringeServiceCollection();
        serviceCollection.AddKeyedSingleton<IGetFootballService, GetFootballService>(FootballKey.Secondary);

        var extension = new MethodInjectionValidationExtension()
            .AddMethod(FootballEndpoints.GetKeyedFootballAsync);

        var invocation = FluentActions.Invoking(() => serviceCollection.BuildSyringeServiceProvider(options =>
        {
            options.WithAfterGetService(extension);
            options.ValidateOnBuild = true;
        }));

        invocation.Should().Throw<AggregateException>()
            .WithMessage("Some services are not able to be constructed*")
            .WithInnerException<InvalidOperationException>()
            .WithMessage($"*Unable to resolve required parameter 'service' of type '{typeof(IGetFootballService)}' with key '{FootballKey.Primary}' on method '{typeof(FootballEndpoints).FullName}.{nameof(FootballEndpoints.GetKeyedFootballAsync)}'.*");
    }
}
