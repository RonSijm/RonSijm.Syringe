# Architecture

RonSijm.Syringe extends `Microsoft.Extensions.DependencyInjection` in two layers:

- `RonSijm.Syringe.Lib` defines registration APIs, metadata, and lightweight contracts.
- `RonSijm.Syringe` provides the dynamic service-provider runtime and its extension pipeline.

Fluxor support follows the same split. `RonSijm.Syringe.Fluxor.Lib` contains state-related
contracts and attributes, while `RonSijm.Syringe.Fluxor` contains the runtime integration.

## Component view

```mermaid
flowchart LR
    App["Consumer application"]

    subgraph Syringe["RonSijm.Syringe packages"]
        Lib["RonSijm.Syringe.Lib<br/>registration APIs, attributes,<br/>settings, bootstrap contracts"]
        Core["RonSijm.Syringe<br/>provider runtime, scopes,<br/>dynamic registration, extensions"]
        FluxorLib["RonSijm.Syringe.Fluxor.Lib<br/>feature-state contracts,<br/>ReduceFrom / ReduceInto attributes"]
        FluxorCore["RonSijm.Syringe.Fluxor<br/>Fluxor scanning, reducers,<br/>effects, middleware integration"]
    end

    MicrosoftDI["Microsoft.Extensions.DependencyInjection<br/>abstractions and service descriptors"]
    FluxorRuntime["Fluxor runtime<br/>store, features, reducers,<br/>effects, middleware"]

    Examples["Examples A / B / C"]
    Tests["Syringe tests<br/>Fluxor tests<br/>Microsoft DI compatibility tests"]

    App --> Core
    App -. optional .-> FluxorCore

    Core --> Lib
    Core --> MicrosoftDI
    Lib --> MicrosoftDI

    FluxorCore --> Core
    FluxorCore --> Lib
    FluxorCore --> FluxorLib
    FluxorCore --> FluxorRuntime

    Examples --> Lib
    Tests --> Core
    Tests --> FluxorCore
```

## Runtime view

```mermaid
flowchart TD
    subgraph Inputs["Registration inputs"]
        Explicit["Explicit IServiceCollection registrations"]
        Implicit["WireImplicit assembly scanning"]
        Config["Configuration and registration attributes"]
        Bootstrap["LibraryLoader / IBootstrapper"]
    end

    Collection["SyringeServiceCollection<br/>queued registrations and<br/>BeforeBuild extensions"]
    Descriptors["IServiceCollection<br/>ServiceDescriptor set"]
    Options["SyringeServiceProviderOptions<br/>additional providers and extension lists"]
    Provider["SyringeServiceProvider"]
    Engine["MicrosoftServiceProvider<br/>call-site factory and root scope"]

    Explicit --> Descriptors
    Implicit --> Collection
    Config --> Collection
    Collection --> Descriptors
    Bootstrap -->|LoadServiceDescriptors| Descriptors
    Descriptors --> Provider
    Options --> Provider
    Provider -->|initial build| Engine

    Request["GetService(Type)"] --> Provider
    KeyedRequest["GetKeyedService"] -->|delegates directly| Engine
    Provider --> Override{"Additional provider matches?<br/>Optional, Lazy, singleton,<br/>fallback, open generic"}
    Override -->|yes| OverrideValue["Create override value"]
    Override -->|no| Scope{"Root or child scope?"}
    Scope --> Engine
    Engine --> Resolved["Resolved service"]
    OverrideValue --> Extensions["AfterGetService extensions<br/>property injection and decorators"]
    Resolved --> Extensions
    Extensions --> Caller["Consumer"]

    Dynamic["New runtime descriptors"] -->|LoadServiceDescriptors| Pending["NewServices"]
    Pending -->|Build| Engine
    Pending --> AfterBuild["AfterBuild extensions"]
    AfterBuild -. Fluxor integration .-> Store["Fluxor store<br/>features, effects, middleware"]
```

## Key design points

- Implicit registration is reflection-based and produces standard Microsoft DI
  `ServiceDescriptor` instances.
- The custom provider retains the Microsoft DI call-site engine, then adds runtime descriptor
  loading, custom scopes, special-type providers, and extension hooks around it.
- `LoadServiceDescriptors` stages new registrations. `Build` adds them to the existing call-site
  factory and invalidates stale accessors without replacing existing scopes.
- Resolution extensions run after a service is created. Build extensions receive the descriptors
  added by each build and can integrate them with external runtimes such as Fluxor.
- The `*.Lib` projects keep contracts usable by libraries without requiring the corresponding
  runtime package.
