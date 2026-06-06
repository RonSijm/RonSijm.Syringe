using RonSijm.Syringe.Tests.Features.TestHelpers;

namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.MethodInjection;

public interface IGetFootballService
{
    Task<string> GetFootballAsync(int footballId, CancellationToken cancellationToken);
}

public class GetFootballService : IGetFootballService
{
    public Task<string> GetFootballAsync(int footballId, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Football-{footballId}");
    }
}

public enum FootballKey
{
    Primary,
    Secondary
}

public static class FootballEndpoints
{
    public static async Task<string> GetFootballAsync(
        int footballId,
        [FromServices] IGetFootballService service,
        CancellationToken cancellationToken)
    {
        return await service.GetFootballAsync(footballId, cancellationToken);
    }

    public static async Task<string> GetKeyedFootballAsync(
        int footballId,
        [FromKeyedServices(FootballKey.Primary)] IGetFootballService service,
        CancellationToken cancellationToken)
    {
        return await service.GetFootballAsync(footballId, cancellationToken);
    }
}