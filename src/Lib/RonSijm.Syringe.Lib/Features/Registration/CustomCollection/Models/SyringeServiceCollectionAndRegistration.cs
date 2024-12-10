using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public class SyringeServiceCollectionAndRegistration : IHaveServiceCollection
{
    public SyringeServiceCollection Collection { get; init; }
    internal SyringeServiceDescriptor Descriptor { get; set; }

    public IServiceProvider BuildServiceProvider()
    {
        return Collection.BuildServiceProvider();
    }

    public IServiceCollection InnerServiceCollection => Collection.InnerServiceCollection;
    public Assembly Assembly { get; set; }

    public static implicit operator SyringeServiceCollection(SyringeServiceCollectionAndRegistration source)
    {
        return source.Collection;
    }
}