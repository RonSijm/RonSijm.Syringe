using RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.BootStapping.SampleObjects;
using RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.Scopes;

namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.BootStapping;

public class SyringeServiceProvider_ScopedInstancePreservationTests
{
    /// <summary>
    /// This test verifies that scoped service instances are preserved after Build() is called.
    /// 
    /// This is important for scenarios like Blazor WASM with lazy loading:
    /// - RadzenDialog subscribes to DialogService.OnOpen in OnInitialized()
    /// - When Blazyload loads a new assembly and calls Build(), the DialogService instance
    ///   in existing scopes must remain the same, otherwise event subscriptions break.
    /// 
    /// The bug was that DoAfterBuild() was replacing scoped providers with new instances,
    /// which caused new components to get a different DialogService instance than the one
    /// RadzenDialog subscribed to.
    /// </summary>
    [Fact]
    public async Task ScopedServiceInstancePreservedAfterBuild()
    {
        // Arrange - Create a service provider with a scoped service
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<ClassWithGuid>();
        var serviceProvider = new Syringe.SyringeServiceProvider(serviceCollection);

        // Create a scope and resolve the scoped service
        var scope = serviceProvider.CreateScope();
        var scopedBefore = scope.ServiceProvider.GetService<ClassWithGuid>();
        scopedBefore.Should().NotBeNull();

        // Act - Add new services and rebuild (simulates Blazyload loading a new assembly)
        var serviceCollection2 = new ServiceCollection();
        serviceCollection2.AddScoped<SimpleSampleObject1>();
        await serviceProvider.LoadServiceDescriptors(serviceCollection2);
        serviceProvider.Build();

        // Assert - The same scoped instance should be returned from the same scope
        var scopedAfter = scope.ServiceProvider.GetService<ClassWithGuid>();
        scopedAfter.Should().NotBeNull();
        
        // This is the critical assertion - the instance must be the same
        // If this fails, event subscriptions would break (like RadzenDialog's subscription to DialogService.OnOpen)
        scopedBefore!.Id.Should().Be(scopedAfter!.Id, 
            because: "scoped service instances must be preserved after Build() to maintain event subscriptions");
    }

    /// <summary>
    /// Verifies that new services can be resolved from existing scopes after Build().
    /// </summary>
    [Fact]
    public async Task NewServicesCanBeResolvedFromExistingScopeAfterBuild()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<ClassWithGuid>();
        var serviceProvider = new Syringe.SyringeServiceProvider(serviceCollection);

        var scope = serviceProvider.CreateScope();
        var existingService = scope.ServiceProvider.GetService<ClassWithGuid>();
        existingService.Should().NotBeNull();

        // New service should not be resolvable yet
        var newServiceBefore = scope.ServiceProvider.GetService<SimpleSampleObject1>();
        newServiceBefore.Should().BeNull();

        // Act - Add new services and rebuild
        var serviceCollection2 = new ServiceCollection();
        serviceCollection2.AddScoped<SimpleSampleObject1>();
        await serviceProvider.LoadServiceDescriptors(serviceCollection2);
        serviceProvider.Build();

        // Assert - New service should now be resolvable from the existing scope
        var newServiceAfter = scope.ServiceProvider.GetService<SimpleSampleObject1>();
        newServiceAfter.Should().NotBeNull(because: "new services should be resolvable from existing scopes after Build()");
    }

    /// <summary>
    /// Verifies that scoped services in different scopes remain independent after Build().
    /// </summary>
    [Fact]
    public async Task DifferentScopesRemainIndependentAfterBuild()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<ClassWithGuid>();
        var serviceProvider = new Syringe.SyringeServiceProvider(serviceCollection);

        var scope1 = serviceProvider.CreateScope();
        var scope2 = serviceProvider.CreateScope();
        
        var scope1Before = scope1.ServiceProvider.GetService<ClassWithGuid>();
        var scope2Before = scope2.ServiceProvider.GetService<ClassWithGuid>();
        
        // Different scopes should have different instances
        scope1Before!.Id.Should().NotBe(scope2Before!.Id);

        // Act - Add new services and rebuild
        var serviceCollection2 = new ServiceCollection();
        serviceCollection2.AddScoped<SimpleSampleObject1>();
        await serviceProvider.LoadServiceDescriptors(serviceCollection2);
        serviceProvider.Build();

        // Assert - Each scope should still return its own instance
        var scope1After = scope1.ServiceProvider.GetService<ClassWithGuid>();
        var scope2After = scope2.ServiceProvider.GetService<ClassWithGuid>();
        
        scope1Before.Id.Should().Be(scope1After!.Id, because: "scope1 should preserve its instance");
        scope2Before.Id.Should().Be(scope2After!.Id, because: "scope2 should preserve its instance");
        scope1After.Id.Should().NotBe(scope2After.Id, because: "different scopes should remain independent");
    }
}
