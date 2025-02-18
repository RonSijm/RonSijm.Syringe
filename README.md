# RonSijm.Syringe

A library for to make it easier to do dependency injection.

Get it? A Syringe is used to inject things. Yes jokes are better when you have to explain them...

[![codecov](https://codecov.io/github/RonSijm/RonSijm.Syringe/graph/badge.svg?token=QFGV300KRH)](https://codecov.io/github/RonSijm/RonSijm.Syringe)


## Default ServiceCollection Extensions:

<!-- snippet: CodeExample-DefaultServiceCollection-WireByType -->
<a id='snippet-CodeExample-DefaultServiceCollection-WireByType'></a>
```cs
var serviceCollection = new ServiceCollection();
serviceCollection.WireImplicit(typeof(ClassA));
var serviceProvider = serviceCollection.BuildServiceProvider();
```
<sup><a href='/src/Tests/RonSijm.Syringe.Tests/Features/Tests/MicrosoftServiceProvider/Registration/DefaultServiceCollection/InitiationMethods/TestWireImplicit_Type.cs#L10-L16' title='Snippet source file'>snippet source</a> | <a href='#snippet-CodeExample-DefaultServiceCollection-WireByType' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- snippet: CodeExample-DefaultServiceCollection-WireByTypeGeneric -->
<a id='snippet-CodeExample-DefaultServiceCollection-WireByTypeGeneric'></a>
```cs
var serviceCollection = new ServiceCollection();
serviceCollection.WireImplicit<ClassA>();
var serviceProvider = serviceCollection.BuildServiceProvider();
```
<sup><a href='/src/Tests/RonSijm.Syringe.Tests/Features/Tests/MicrosoftServiceProvider/Registration/DefaultServiceCollection/InitiationMethods/TestWireImplicit_GenericType.cs#L10-L16' title='Snippet source file'>snippet source</a> | <a href='#snippet-CodeExample-DefaultServiceCollection-WireByTypeGeneric' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- snippet: CodeExample-DefaultServiceCollection-WireByAssembly -->
<a id='snippet-CodeExample-DefaultServiceCollection-WireByAssembly'></a>
```cs
var serviceCollection = new ServiceCollection();
var assembly = typeof(ClassA).Assembly;
serviceCollection.WireImplicit(assembly, ServiceLifetime.Transient, []);
var serviceProvider = serviceCollection.BuildServiceProvider();
```
<sup><a href='/src/Tests/RonSijm.Syringe.Tests/Features/Tests/MicrosoftServiceProvider/Registration/DefaultServiceCollection/InitiationMethods/TestWireImplicit_Assembly.cs#L10-L15' title='Snippet source file'>snippet source</a> | <a href='#snippet-CodeExample-DefaultServiceCollection-WireByAssembly' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Scoped: (default)

<!-- snippet: CodeExample-DefaultScope -->
<a id='snippet-CodeExample-DefaultScope'></a>
```cs
return typeof(ClassWithGuidA).WireImplicit().BuildServiceProvider();
```
<sup><a href='/src/Tests/RonSijm.Syringe.Tests/Features/Tests/MicrosoftServiceProvider/Registration/Attributes/LifetimeTests/LifetimeTests_Scoped.cs#L10-L12' title='Snippet source file'>snippet source</a> | <a href='#snippet-CodeExample-DefaultScope' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Transient:

<!-- snippet: CodeExample-TransientScope -->
<a id='snippet-CodeExample-TransientScope'></a>
```cs
return typeof(ClassWithGuidA).WireImplicit().WithLifetime(ServiceLifetime.Transient).BuildServiceProvider();
```
<sup><a href='/src/Tests/RonSijm.Syringe.Tests/Features/Tests/MicrosoftServiceProvider/Registration/Attributes/LifetimeTests/LifetimeTests_Transient.cs#L10-L12' title='Snippet source file'>snippet source</a> | <a href='#snippet-CodeExample-TransientScope' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Singleton:

<!-- snippet: CodeExample-SingletonScope -->
<a id='snippet-CodeExample-SingletonScope'></a>
```cs
return typeof(ClassWithGuidA).WireImplicit().WithLifetime(ServiceLifetime.Singleton).BuildServiceProvider();
```
<sup><a href='/src/Tests/RonSijm.Syringe.Tests/Features/Tests/MicrosoftServiceProvider/Registration/Attributes/LifetimeTests/LifetimeTests_Singleton.cs#L10-L12' title='Snippet source file'>snippet source</a> | <a href='#snippet-CodeExample-SingletonScope' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: CodeExample-Parameters-registerAsTypesOnly -->
<a id='snippet-CodeExample-Parameters-registerAsTypesOnly'></a>
```cs
var serviceCollection = new ServiceCollection();
serviceCollection.WireImplicit<ClassA>(registerAsTypesOnly: [typeof(ClassWithInterfaceA)]);
var serviceProvider = serviceCollection.BuildServiceProvider();
```
<sup><a href='/src/Tests/RonSijm.Syringe.Tests/Features/Tests/MicrosoftServiceProvider/Registration/DefaultServiceCollection/Parameters/TestWireImplicit_RegisterAsTypesOnly_ClassWithInterface.cs#L11-L15' title='Snippet source file'>snippet source</a> | <a href='#snippet-CodeExample-Parameters-registerAsTypesOnly' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- snippet: CodeExample-Parameters-registerBothTypeAndInterfaces -->
<a id='snippet-CodeExample-Parameters-registerBothTypeAndInterfaces'></a>
```cs
var serviceCollection = new ServiceCollection();
serviceCollection.WireImplicit<ClassA>(registerBothTypeAndInterfaces: [typeof(ClassWithInterfaceA)]);
var serviceProvider = serviceCollection.BuildServiceProvider();
```
<sup><a href='/src/Tests/RonSijm.Syringe.Tests/Features/Tests/MicrosoftServiceProvider/Registration/DefaultServiceCollection/Parameters/TestWireImplicit_RegisterBothTypeAndInterfaces_ClassWithInterface.cs#L10-L14' title='Snippet source file'>snippet source</a> | <a href='#snippet-CodeExample-Parameters-registerBothTypeAndInterfaces' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Extension Methods

By doing a `WireImplicit` - The extension will return a `SyringeServiceCollectionAndRegistration` object.
This is an object that keeps track of the last registion that you've made, allowing you to append extra configuration to that specific registration:

<!-- snippet: CodeExample-DontRegisterTypesExtension -->
<a id='snippet-CodeExample-DontRegisterTypesExtension'></a>
```cs
var serviceCollection = new SyringeServiceCollection();
serviceCollection.WireImplicit<ClassA>().DontRegisterTypes(typeof(ClassA));
```
<sup><a href='/src/Tests/RonSijm.Syringe.Tests/Features/Tests/MicrosoftServiceProvider/CustomCollection/Parameters/TestWireImplicit_DontRegisterAsTypes_Class.cs#L11-L14' title='Snippet source file'>snippet source</a> | <a href='#snippet-CodeExample-DontRegisterTypesExtension' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- snippet: CodeExample-DontRegisterTypesWithInterfaces -->
<a id='snippet-CodeExample-DontRegisterTypesWithInterfaces'></a>
```cs
var serviceCollection = new SyringeServiceCollection();
serviceCollection.WireImplicit<ClassA>().DontRegisterTypesWithInterfaces([typeof(InterfaceFor_ClassWithInterfaceA)]);
```
<sup><a href='/src/Tests/RonSijm.Syringe.Tests/Features/Tests/MicrosoftServiceProvider/CustomCollection/Parameters/TestWireImplicit_DontRegisterTypesWithInterface_InterfaceFor_ClassWithInterface.cs#L11-L14' title='Snippet source file'>snippet source</a> | <a href='#snippet-CodeExample-DontRegisterTypesWithInterfaces' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- snippet: CodeExample-RegisterAsTypesOnly -->
<a id='snippet-CodeExample-RegisterAsTypesOnly'></a>
```cs
var serviceCollection = new SyringeServiceCollection();
serviceCollection.WireImplicit<ClassA>().RegisterAsTypesOnly([typeof(ClassWithInterfaceA)]);
```
<sup><a href='/src/Tests/RonSijm.Syringe.Tests/Features/Tests/MicrosoftServiceProvider/CustomCollection/Parameters/TestWireImplicit_RegisterAsTypesOnly_ClassWithInterface.cs#L11-L14' title='Snippet source file'>snippet source</a> | <a href='#snippet-CodeExample-RegisterAsTypesOnly' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- snippet: CodeExample-RegisterBothTypeAndInterfaces -->
<a id='snippet-CodeExample-RegisterBothTypeAndInterfaces'></a>
```cs
var serviceCollection = new SyringeServiceCollection();
serviceCollection.WireImplicit<ClassA>().RegisterBothTypeAndInterfaces([typeof(ClassWithInterfaceA)]);
```
<sup><a href='/src/Tests/RonSijm.Syringe.Tests/Features/Tests/MicrosoftServiceProvider/CustomCollection/Parameters/TestWireImplicit_RegisterBothTypeAndInterfaces_ClassWithInterface.cs#L10-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-CodeExample-RegisterBothTypeAndInterfaces' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Registration Extensions

A `SyringeServiceCollection` contains a `internal List<IWireSyringeServiceDescriptorExtension> Extensions { get; set; } = [];` - which lets you add your own `BeforeBuildServiceProvider` Extensions

### Example

You can put these kind of configurations in your `appsettings.json`:

#### Don't register classes with this interface:

Usecase:

Imagine you have an `IProcessor` (Interface), `DatabaseProcessor` and `APIProcessor` that implement this interface - In your appsettings.json you can register or unregister a specific implementation for this interface.

<!-- snippet: CodeExample-ConfigurationNotWithInterfaces -->
<a id='snippet-CodeExample-ConfigurationNotWithInterfaces'></a>
```cs
var appsetting = """
                 {
                     "DependencyInjection": {
                         "Assembly:RonSijm.Syringe.ExamplesC": {
                         "Type:ExampleCInterface": "None"
                         }
                     }
                 }
                 """.ToConfiguration();
```
<sup><a href='/src/Tests/RonSijm.Syringe.Tests/Features/Tests/MicrosoftServiceProvider/Registration/Config/ExamplesC_InterfaceRegistrationNone.cs#L11-L23' title='Snippet source file'>snippet source</a> | <a href='#snippet-CodeExample-ConfigurationNotWithInterfaces' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

#### Don't register these specific classes:

Usecase:

Imagine you have an `IProcessor` (Interface), `DatabaseProcessor` and `APIProcessor` and `UserProcessor` - In your appsettings.json you can register or unregister a specific implementations for this interface.

When your consumer takes an `IEnumerable<IProcessor>` - you can register or unregister specific processors, and only run selected processors.

<!-- snippet: CodeExample-ConfigurationNotSpecificClass -->
<a id='snippet-CodeExample-ConfigurationNotSpecificClass'></a>
```cs
var appsetting = """
                 {
                     "DependencyInjection": {
                         "Assembly:RonSijm.Syringe.ExamplesC": {
                         "Type:ExampleC2": "None",
                         "Type:ExampleC3": "None",
                         }
                     }
                 }
                 """.ToConfiguration();
```
<sup><a href='/src/Tests/RonSijm.Syringe.Tests/Features/Tests/MicrosoftServiceProvider/Registration/Config/ExamplesC_TypeRegistrationNone.cs#L11-L24' title='Snippet source file'>snippet source</a> | <a href='#snippet-CodeExample-ConfigurationNotSpecificClass' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
