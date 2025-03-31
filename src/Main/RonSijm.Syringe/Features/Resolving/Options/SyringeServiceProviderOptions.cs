using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public class SyringeServiceProviderOptions : ServiceProviderOptions
{
    public Func<IServiceCollection, MicrosoftServiceProvider> ServiceProviderBuilder { get; set; }
    public readonly List<object> ExtendedOptions = new();
    public bool BuildOnConstruct { get; set; } = true;

    public SyringeServiceCollection Services { get; } = new();
    public List<AdditionProvider> AdditionalProviders { get; } = new();
    public List<ISyringeServiceProviderAfterServiceExtension> AfterGetServiceExtensions { get; set; } = [];
    public List<ISyringeAfterBuildExtension> AfterBuildExtensions { get; set; } = [];

    public ServiceProviderOptions ServiceProviderOptions { get; set; }

    public void WithAfterGetService(ISyringeServiceProviderAfterServiceExtension extension)
    {
        AfterGetServiceExtensions.Add(extension);
    }

    public void WithAfterGetService<T>() where T : ISyringeServiceProviderAfterServiceExtension, new()
    {
        var instance = new T();
        AfterGetServiceExtensions.Add(instance);
    }

    public void WithAfterBuildExtension(ISyringeAfterBuildExtension extension)
    {
        AfterBuildExtensions.Add(extension);
    }

    public void WithAfterBuildExtension<T>() where T : ISyringeAfterBuildExtension, new()
    {
        var instance = new T();
        AfterBuildExtensions.Add(instance);
    }

    public T GetOptions<T>() where T : class
    {
        var options = ExtendedOptions?.FirstOrDefault(x => x is T) as T;
        return options;
    }

    public void AddOption(ServiceProviderOptions option)
    {
        ExtendedOptions.Add(option);
    }
}