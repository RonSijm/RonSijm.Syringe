namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.MethodInjection;

public class TestValidateMethodInjectionOnStockDI
{
    [Fact]
    public void StockDI_DoesNotThrow_WhenServiceRegistered()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IGetFootballService, GetFootballService>();
        var provider = services.BuildServiceProvider();

        var invocation = FluentActions.Invoking(() => provider.ValidateMethodInjection(FootballEndpoints.GetFootballAsync));

        invocation.Should().NotThrow();
    }

    [Fact]
    public void StockDI_Throws_WhenServiceNotRegistered()
    {
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();

        var invocation = FluentActions.Invoking(() => provider.ValidateMethodInjection(FootballEndpoints.GetFootballAsync));

        invocation.Should().Throw<AggregateException>()
            .WithMessage("Some services are not able to be constructed*")
            .WithInnerException<InvalidOperationException>()
            .WithMessage($"*Unable to resolve required parameter 'service' of type '{typeof(IGetFootballService)}' on method '{typeof(FootballEndpoints).FullName}.{nameof(FootballEndpoints.GetFootballAsync)}'.*");
    }

    [Fact]
    public void StockDI_DoesNotThrow_WhenKeyedServiceRegisteredWithCorrectKey()
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IGetFootballService, GetFootballService>(FootballKey.Primary);
        var provider = services.BuildServiceProvider();

        var invocation = FluentActions.Invoking(() => provider.ValidateMethodInjection(FootballEndpoints.GetKeyedFootballAsync));

        invocation.Should().NotThrow();
    }

    [Fact]
    public void StockDI_Throws_WhenKeyedServiceRegisteredWithWrongKey()
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IGetFootballService, GetFootballService>(FootballKey.Secondary);
        var provider = services.BuildServiceProvider();

        var invocation = FluentActions.Invoking(() => provider.ValidateMethodInjection(FootballEndpoints.GetKeyedFootballAsync));

        invocation.Should().Throw<AggregateException>()
            .WithMessage("Some services are not able to be constructed*")
            .WithInnerException<InvalidOperationException>()
            .WithMessage($"*Unable to resolve required parameter 'service' of type '{typeof(IGetFootballService)}' with key '{FootballKey.Primary}' on method '{typeof(FootballEndpoints).FullName}.{nameof(FootballEndpoints.GetKeyedFootballAsync)}'.*");
    }

    [Fact]
    public void StockDI_AggregatesAllMissingDependencies()
    {
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();

        var invocation = FluentActions.Invoking(() => provider.ValidateMethodInjection(
            FootballEndpoints.GetFootballAsync,
            FootballEndpoints.GetKeyedFootballAsync));

        var assertion = invocation.Should().Throw<AggregateException>().Which;
        assertion.InnerExceptions.Should().HaveCount(2);
        assertion.InnerExceptions.Should().AllBeOfType<InvalidOperationException>();
    }
}
