using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public class SyringeServiceProviderOptions
{
    public Func<IServiceCollection, IServiceProvider> ServiceProviderBuilder { get; set; }
    private readonly List<ServiceProviderOptions> _extendedOptions = new();
    public bool BuildOnConstruct { get; set; } = true;

    public SyringeServiceCollection Services { get; set; }

    public List<ISyringeServiceProviderAfterServiceExtension> AfterGetServiceExtensions { get; set; } = [];
    public ServiceProviderOptions ServiceProviderOptions { get; set; }

    public void WithAfterGetServiceExtension(ISyringeServiceProviderAfterServiceExtension extension)
    {
        AfterGetServiceExtensions.Add(extension);
    }

    public T GetOptions2<T>() where T : ServiceProviderOptions
    {
        var options = _extendedOptions.FirstOrDefault(x => x is T) as T;
        return options;
    }

    public void AddOption(ServiceProviderOptions option)
    {
        _extendedOptions.Add(option);
    }
}