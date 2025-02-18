using RonSijm.Syringe.ExamplesB;

namespace RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesB;

public abstract class ResolveExamplesBNoExceptionsBase : ServiceProviderSetupBase
{
    [Fact]
    public void Resolve_ClassWith_FuncOfClass()
    {
        var invocation = ServiceProvider.Invoking(sp => sp.GetRequiredService<ClassWith_FuncOfClassB>());
        ClassWith_FuncOfClassExpectations(invocation);
    }

    protected virtual void ClassWith_FuncOfClassExpectations(Func<ClassWith_FuncOfClassB> invocation)
    {
        invocation.Should().NotThrow();
    }

    [Fact]
    public void Resolve_ClassWith_LazyOfClass()
    {
        var invocation = ServiceProvider.Invoking(sp => sp.GetRequiredService<ClassWith_LazyOfClassB>());
        ClassWith_LazyOfClassExpectations(invocation);
    }

    protected virtual void ClassWith_LazyOfClassExpectations(Func<ClassWith_LazyOfClassB> invocation)
    {
        invocation.Should().NotThrow();
    }
}