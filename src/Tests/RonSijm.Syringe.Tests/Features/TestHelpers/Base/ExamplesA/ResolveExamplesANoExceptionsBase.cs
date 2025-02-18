using RonSijm.Syringe.ExamplesA;

namespace RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesA;

public abstract class ResolveExamplesANoExceptionsBase : ServiceProviderSetupBase
{
    [Fact]
    public void Resolve_Class()
    {
        var invocation = ServiceProvider.Invoking(sp => sp.GetRequiredService<ClassA>());
        ClassExpectations(invocation);
    }

    protected virtual void ClassExpectations(Func<ClassA> invocation)
    {
        invocation.Should().NotThrow();
    }

    [Fact]
    public void Resolve_ClassWith_Class()
    {
        var invocation = ServiceProvider.Invoking(sp => sp.GetRequiredService<ClassWith_ClassA>());
        ClassWith_ClassExpectations(invocation);
    }

    protected virtual void ClassWith_ClassExpectations(Func<ClassWith_ClassA> invocation)
    {
        invocation.Should().NotThrow();
    }

    [Fact]
    public void Resolve_ClassWith_ClassWithInterface_AsClass()
    {
        var invocation = ServiceProvider.Invoking(sp => sp.GetRequiredService<ClassWith_ClassWithInterface_AsClassA>());
        ClassWith_ClassWithInterface_AsClassExpectations(invocation);
    }

    protected virtual void ClassWith_ClassWithInterface_AsClassExpectations(Func<ClassWith_ClassWithInterface_AsClassA> invocation)
    {
        invocation.Should().NotThrow();
    }

    [Fact]
    public void Resolve_ClassWith_ClassWithInterface_AsInterface()
    {
        var invocation = ServiceProvider.Invoking(sp => sp.GetRequiredService<ClassWith_ClassWithInterface_AsInterfaceA>());
        ClassWith_ClassWithInterface_AsInterfaceExpectations(invocation);
    }

    protected virtual void ClassWith_ClassWithInterface_AsInterfaceExpectations(Func<ClassWith_ClassWithInterface_AsInterfaceA> invocation)
    {
        invocation.Should().NotThrow();
    }

    [Fact]
    public void Resolve_ClassWithInterface()
    {
        var invocation = ServiceProvider.Invoking(sp => sp.GetRequiredService<ClassWithInterfaceA>());
        ClassWithInterfaceExpectations(invocation);
    }

    protected virtual void ClassWithInterfaceExpectations(Func<ClassWithInterfaceA> invocation)
    {
        invocation.Should().NotThrow();
    }

    [Fact]
    public void Resolve_InterfaceFor_ClassWithInterface()
    {
        var invocation = ServiceProvider.Invoking(sp => sp.GetRequiredService<InterfaceFor_ClassWithInterfaceA>());
        InterfaceFor_ClassWithInterfaceExpectations(invocation);
    }

    protected virtual void InterfaceFor_ClassWithInterfaceExpectations(Func<InterfaceFor_ClassWithInterfaceA> invocation)
    {
        invocation.Should().NotThrow();
    }
}