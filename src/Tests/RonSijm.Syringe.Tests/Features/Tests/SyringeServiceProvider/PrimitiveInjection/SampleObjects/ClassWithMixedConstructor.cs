namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.PrimitiveInjection;

/// <summary>
/// A class that has both a service dependency and primitive parameters
/// </summary>
public class ClassWithMixedConstructor(ISimpleService service, string connectionString, int maxRetries)
{
    public ISimpleService Service { get; private set; } = service;
    public string ConnectionString { get; private set; } = connectionString;
    public int MaxRetries { get; private set; } = maxRetries;
}

public interface ISimpleService
{
    string GetValue();
}

public class SimpleService : ISimpleService
{
    public string GetValue() => "SimpleServiceValue";
}

