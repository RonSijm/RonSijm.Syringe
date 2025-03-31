using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe.Tests.TestObjects.Properties;

// ReSharper disable once UnusedType.Global
public class BlazyBootstrap : IBootstrapper
{
    // ReSharper disable once UnusedMember.Global
    public Task<IEnumerable<ServiceDescriptor>> Bootstrap()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.WireImplicit(typeof(BlazyBootstrap), 

            // Don't have to register the bootstrap itself.
            // It doesn't really matter if you do, I guess, just showing you how to not-do it.
            dontRegisterTypesWithInterfaces: [typeof(IBootstrapper)]);

        return Task.FromResult<IEnumerable<ServiceDescriptor>>(serviceCollection);
    }
}