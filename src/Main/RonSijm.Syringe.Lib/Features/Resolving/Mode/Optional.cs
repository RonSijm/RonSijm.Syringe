namespace RonSijm.Syringe;

/// <summary>
/// A wrapper for a value that can be null.
/// Mostly used in Blazor, because if the ServiceProvider just returns null, the Blazor component will throw an exception.
/// So you can use this to resolve optional dependencies, instead of returning null.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Optional<T> : ModeWrapper<T> where T : class;