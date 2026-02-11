# RonSijm.Syringe

A powerful dependency injection library for .NET that extends Microsoft's built-in DI container with advanced features like implicit wiring, dynamic service registration, optional dependencies, and extensive extensibility points.

Get it? A Syringe is used to inject things. Yes jokes are better when you have to explain them...

[![codecov](https://codecov.io/github/RonSijm/RonSijm.Syringe/graph/badge.svg?token=QFGV300KRH)](https://codecov.io/github/RonSijm/RonSijm.Syringe)

## Features Overview

- **Implicit Wiring** - Automatically register all classes in an assembly with a single line of code
- **Dynamic Service Registration** - Add services at runtime after the container is built
- **Optional Dependencies** - `Optional<T>` wrapper for dependencies that may not be available
- **Lazy Resolution** - `Lazy<T>` support for deferred service instantiation
- **Attribute-Based Registration** - Control registration behavior with attributes like `[DontRegister]` and `[Singleton]`
- **Configuration-Based Registration** - Configure service registration via `appsettings.json`
- **Assembly Bootstrapping** - `IBootstrapper` interface for library initialization
- **Extensibility Points** - Hook into service resolution and build events
- **Keyed Services** - Full support for .NET 8+ keyed services
- **Scoped Services** - Proper scope management with `CreateScope()`

---

## Installation

```bash
dotnet add package RonSijm.Syringe
```

---

## Quick Start

The simplest way to use Syringe is with implicit wiring:

```csharp
var services = new ServiceCollection();
services.WireImplicit<MyClass>(); // Registers all classes in the assembly containing MyClass
var provider = services.BuildServiceProvider();
```

---

## Default ServiceCollection Extensions:

snippet: CodeExample-DefaultServiceCollection-WireByType
snippet: CodeExample-DefaultServiceCollection-WireByTypeGeneric
snippet: CodeExample-DefaultServiceCollection-WireByAssembly

Scoped: (default)

snippet: CodeExample-DefaultScope

Transient:

snippet: CodeExample-TransientScope

Singleton:

snippet: CodeExample-SingletonScope

snippet: CodeExample-Parameters-registerAsTypesOnly
snippet: CodeExample-Parameters-registerBothTypeAndInterfaces

## Extension Methods

By doing a `WireImplicit` - The extension will return a `SyringeServiceCollectionAndRegistration` object.
This is an object that keeps track of the last registion that you've made, allowing you to append extra configuration to that specific registration:

snippet: CodeExample-DontRegisterTypesExtension
snippet: CodeExample-DontRegisterTypesWithInterfaces
snippet: CodeExample-RegisterAsTypesOnly
snippet: CodeExample-RegisterBothTypeAndInterfaces


## Registration Extensions

A `SyringeServiceCollection` contains a `internal List<IWireSyringeServiceDescriptorExtension> Extensions { get; set; } = [];` - which lets you add your own `BeforeBuildServiceProvider` Extensions

### Example

You can put these kind of configurations in your `appsettings.json`:

#### Don't register classes with this interface:

Usecase:

Imagine you have an `IProcessor` (Interface), `DatabaseProcessor` and `APIProcessor` that implement this interface - In your appsettings.json you can register or unregister a specific implementation for this interface.

snippet: CodeExample-ConfigurationNotWithInterfaces

#### Don't register these specific classes:

Usecase:

Imagine you have an `IProcessor` (Interface), `DatabaseProcessor` and `APIProcessor` and `UserProcessor` - In your appsettings.json you can register or unregister a specific implementations for this interface.

When your consumer takes an `IEnumerable<IProcessor>` - you can register or unregister specific processors, and only run selected processors.

snippet: CodeExample-ConfigurationNotSpecificClass

---

## SyringeServiceProvider

The `SyringeServiceProvider` is an enhanced service provider that wraps Microsoft's DI container and adds powerful features like dynamic service registration at runtime.

### Basic Usage

```csharp
// Create with options
var provider = new SyringeServiceProvider(options =>
{
    options.BuildOnConstruct = true; // Build immediately (default)
    options.Services.WireImplicit<MyClass>();
});

// Or create from an existing service collection
var services = new ServiceCollection();
services.AddSingleton<IMyService, MyService>();
var provider = new SyringeServiceProvider(services);
```

### Dynamic Service Registration

One of the most powerful features is the ability to add services after the container is built:

```csharp
var provider = new SyringeServiceProvider();

// Later, dynamically load more services
var newServices = new ServiceCollection();
newServices.AddSingleton<INewService, NewService>();
await provider.LoadServiceDescriptors(newServices);
provider.Build(); // Rebuild to include new services
```

This is particularly useful for plugin architectures or lazy-loading scenarios.

---

## Optional Dependencies

The `Optional<T>` wrapper allows you to inject dependencies that may not be registered. This is especially useful in Blazor where injecting `null` throws an exception.

```csharp
public class MyComponent
{
    private readonly Optional<IOptionalService> _optionalService;

    public MyComponent(Optional<IOptionalService> optionalService)
    {
        _optionalService = optionalService;
    }

    public void DoWork()
    {
        if (_optionalService.HasValue)
        {
            _optionalService.Value.DoSomething();
        }
    }
}
```

`Optional<T>` is automatically registered when using `SyringeServiceProvider`.

---

## Lazy Resolution

Syringe supports `Lazy<T>` for deferred service instantiation:

```csharp
public class MyService
{
    private readonly Lazy<IExpensiveService> _expensiveService;

    public MyService(Lazy<IExpensiveService> expensiveService)
    {
        _expensiveService = expensiveService;
    }

    public void DoWork()
    {
        // Service is only created when first accessed
        _expensiveService.Value.Process();
    }
}
```

---

## Attribute-Based Registration

Control how classes are registered using attributes:

### Don't Register Attribute

Prevent a class from being automatically registered:

```csharp
[Registration.DontRegister]
public class InternalHelper
{
    // This class won't be registered by WireImplicit
}
```

### Lifetime Attributes

Specify the service lifetime:

```csharp
[Lifetime.Singleton]
public class MySingletonService : IMyService
{
    // Will be registered as singleton
}
```

---

## IBootstrapper Interface

The `IBootstrapper` interface allows libraries to define their own service registrations that are automatically discovered and executed:

```csharp
public class MyLibraryBootstrapper : IBootstrapper
{
    public Task<IEnumerable<ServiceDescriptor>> Bootstrap()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IMyLibraryService, MyLibraryService>();
        services.AddScoped<IRepository, Repository>();

        return Task.FromResult<IEnumerable<ServiceDescriptor>>(services);
    }
}
```

When an assembly is loaded, Syringe automatically discovers and executes all `IBootstrapper` implementations.

---

## ILoadAfterExtension

Hook into the assembly loading process with `ILoadAfterExtension`:

```csharp
public class MyLoadExtension : BaseLoadAfterExtension
{
    public override void AssembliesLoaded(List<Assembly> loadedAssemblies)
    {
        // Called after assemblies are loaded
        Console.WriteLine($"Loaded {loadedAssemblies.Count} assemblies");
    }

    public override void DescriptorsLoaded(List<ServiceDescriptor> loadedDescriptors)
    {
        // Called after service descriptors are registered
        Console.WriteLine($"Registered {loadedDescriptors.Count} services");
    }
}
```

---

## Extensibility Points

### ISyringeAfterBuildExtension

Execute code after the service provider is built:

```csharp
public class MyAfterBuildExtension : ISyringeAfterBuildExtension
{
    private SyringeServiceProvider _provider;

    public void SetReference(SyringeServiceProvider serviceProvider)
    {
        _provider = serviceProvider;
    }

    public void Process(List<ServiceDescriptor> loadedDescriptors, bool isInitialBuild)
    {
        // Called after Build() is called
    }
}

// Register the extension
var provider = new SyringeServiceProvider(options =>
{
    options.WithAfterBuildExtension<MyAfterBuildExtension>();
});
```

### ISyringeServiceProviderAfterServiceExtension

Decorate or modify services after they are resolved:

```csharp
public class MyAfterServiceExtension : ISyringeServiceProviderAfterServiceExtension
{
    public void SetReference(SyringeServiceProvider serviceProvider) { }

    public void Decorate(Type serviceType, object service)
    {
        // Called after every service resolution
    }
}

// Register the extension
var provider = new SyringeServiceProvider(options =>
{
    options.WithAfterGetService<MyAfterServiceExtension>();
});
```

---

## Keyed Services

Syringe fully supports .NET 8+ keyed services:

```csharp
var provider = new SyringeServiceProvider(services =>
{
    services.AddKeyedSingleton<IMyService, ServiceA>("A");
    services.AddKeyedSingleton<IMyService, ServiceB>("B");
});

var serviceA = provider.GetKeyedService<IMyService>("A");
var serviceB = provider.GetKeyedService<IMyService>("B");
```

---

## Scoped Services

Create and manage scopes properly:

```csharp
var provider = new SyringeServiceProvider();

using (var scope = provider.CreateScope())
{
    var scopedService = scope.ServiceProvider.GetService<IMyScopedService>();
    // Use scoped service
}
// Scope is disposed, scoped services are cleaned up
```

---

## Global Settings

Configure default behavior globally:

```csharp
// Set default service lifetime (default is Scoped)
SyringeGlobalSettings.DefaultServiceLifetime = ServiceLifetime.Transient;

// Control whether types with interfaces are also registered as themselves
SyringeGlobalSettings.RegisterAsTypeWhenTypeHasInterfaces = true;
```

---

## Fluxor Integration

Syringe includes optional Fluxor integration for state management:

```csharp
var provider = new SyringeServiceProvider(options =>
{
    options.UseFluxor(fluxorOptions =>
    {
        fluxorOptions.ScanAssemblies(typeof(Program).Assembly);
    });
});
```

Install the Fluxor integration package:

```bash
dotnet add package RonSijm.Syringe.Fluxor
```

---

## Related Projects

- **[RonSijm.Blazyload](https://github.com/RonSijm/RonSijm.Blazyload)** - Lazy-loading for Blazor WebAssembly using Syringe