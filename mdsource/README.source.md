# RonSijm.Syringe

A library for to make it easier to do dependency injection.

Get it? A Syringe is used to inject things. Yes jokes are better when you have to explain them...

[![codecov](https://codecov.io/github/RonSijm/RonSijm.Syringe/graph/badge.svg?token=QFGV300KRH)](https://codecov.io/github/RonSijm/RonSijm.Syringe)


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