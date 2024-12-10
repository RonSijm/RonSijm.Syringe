using RonSijm.Syringe.ExamplesA;

namespace RonSijm.Syringe.Tests.Features.TestHelpers.Base.ExamplesA;

public abstract class ResolveExamplesANoExceptionsBase : ServiceProviderSetupBase
{
    [Fact]
    public void Resolve_Class()
    {
        var invocation = ServiceProvider.Invoking(sp => sp.GetRequiredService<Class>());
        ClassExpectations(invocation);
    }

    protected virtual void ClassExpectations(Func<Class> invocation)
    {
        invocation.Should().NotThrow();
    }

    [Fact]
    public void Resolve_ClassWith_Class()
    {
        var invocation = ServiceProvider.Invoking(sp => sp.GetRequiredService<ClassWith_Class>());
        ClassWith_ClassExpectations(invocation);
    }

    protected virtual void ClassWith_ClassExpectations(Func<ClassWith_Class> invocation)
    {
        invocation.Should().NotThrow();
    }

    [Fact]
    public void Resolve_ClassWith_ClassWithInterface_AsClass()
    {
        var invocation = ServiceProvider.Invoking(sp => sp.GetRequiredService<ClassWith_ClassWithInterface_AsClass>());
        ClassWith_ClassWithInterface_AsClassExpectations(invocation);
    }

    protected virtual void ClassWith_ClassWithInterface_AsClassExpectations(Func<ClassWith_ClassWithInterface_AsClass> invocation)
    {
        invocation.Should().NotThrow();
    }

    [Fact]
    public void Resolve_ClassWith_ClassWithInterface_AsInterface()
    {
        var invocation = ServiceProvider.Invoking(sp => sp.GetRequiredService<ClassWith_ClassWithInterface_AsInterface>());
        ClassWith_ClassWithInterface_AsInterfaceExpectations(invocation);
    }

    protected virtual void ClassWith_ClassWithInterface_AsInterfaceExpectations(Func<ClassWith_ClassWithInterface_AsInterface> invocation)
    {
        invocation.Should().NotThrow();
    }

    [Fact]
    public void Resolve_ClassWithInterface()
    {
        var invocation = ServiceProvider.Invoking(sp => sp.GetRequiredService<ClassWithInterface>());
        ClassWithInterfaceExpectations(invocation);
    }

    protected virtual void ClassWithInterfaceExpectations(Func<ClassWithInterface> invocation)
    {
        invocation.Should().NotThrow();
    }

    [Fact]
    public void Resolve_InterfaceFor_ClassWithInterface()
    {
        var invocation = ServiceProvider.Invoking(sp => sp.GetRequiredService<InterfaceFor_ClassWithInterface>());
        InterfaceFor_ClassWithInterfaceExpectations(invocation);
    }

    protected virtual void InterfaceFor_ClassWithInterfaceExpectations(Func<InterfaceFor_ClassWithInterface> invocation)
    {
        invocation.Should().NotThrow();
    }
}