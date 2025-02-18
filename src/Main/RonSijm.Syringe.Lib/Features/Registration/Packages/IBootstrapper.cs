// ReSharper disable global UnusedMember.Global

using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

/// <summary>
/// Interface for bootstrapped classes
/// </summary>
public interface IBootstrapper
{
    /// <summary>
    /// Method that can be used to bootstrap libraries.
    /// Can be used to return dependencies to inject in the DI framework,
    /// Or can also be used to bootstrap other stuff in your library as it's loaded
    /// </summary>
    /// <returns>The services to inject in the DI</returns>
    Task<IEnumerable<ServiceDescriptor>> Bootstrap();
}