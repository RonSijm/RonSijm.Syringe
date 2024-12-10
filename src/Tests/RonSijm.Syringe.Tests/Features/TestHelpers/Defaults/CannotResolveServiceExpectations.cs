namespace RonSijm.Syringe.Tests.Features.TestHelpers.Defaults;

public static class CannotResolveServiceExpectations
{
    public static void UnableToResolveServiceExpectation<T>(this Func<T> invocation, Type source)
    {
        invocation.Should().Throw<InvalidOperationException>().WithMessage($"Unable to resolve service for type '{source}' while attempting to activate '{typeof(T).FullName}'.");
    }

    public static void UnableToResolveServiceExpectation<T>(this Func<T> invocation, string source)
    {
        invocation.Should().Throw<InvalidOperationException>().WithMessage($"Unable to resolve service for type '{source}' while attempting to activate '{typeof(T).FullName}'.");
    }
}