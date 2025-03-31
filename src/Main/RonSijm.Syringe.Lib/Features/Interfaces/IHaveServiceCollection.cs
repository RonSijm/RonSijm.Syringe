using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe.Interfaces;

/// <summary>
/// Interface to represent a class that has an <see cref="IServiceCollection"/>.
/// </summary>
public interface IHaveServiceCollection
{
    internal IServiceCollection InnerServiceCollection { get; }
}