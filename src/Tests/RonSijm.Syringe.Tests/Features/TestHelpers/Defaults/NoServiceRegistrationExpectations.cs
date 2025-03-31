namespace RonSijm.Syringe.Tests.Features.TestHelpers.Defaults;

public static class NoServiceRegistrationExpectations
{
    public static void NoRegistrationExpectation<T>(this Func<T> invocation)
    {
        invocation.Should().Throw<InvalidOperationException>().WithMessage($"No service for type '{typeof(T).FullName}' has been registered.");
    }

    public static void NoRegistrationExpectation<T>(this Func<T> invocation, string typeName)
    {
        invocation.Should().Throw<InvalidOperationException>().WithMessage($"No service for type '{typeName}' has been registered.");
    }
}