using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe;

public class SyringeServiceProviderOptions
{
    public Func<IServiceCollection, MicrosoftServiceProvider> ServiceProviderBuilder { get; set; }
    private readonly List<ServiceProviderOptions> _extendedOptions = new();
    public bool BuildOnConstruct { get; set; } = true;

    public SyringeServiceCollection Services { get; set; }

    public List<AdditionProvider> AdditionalProviders { get; } = new();
    public List<ISyringeServiceProviderAfterServiceExtension> AfterGetServiceExtensions { get; set; } = [];
    public ServiceProviderOptions ServiceProviderOptions { get; set; }

    public void WithAfterGetServiceExtension(ISyringeServiceProviderAfterServiceExtension extension)
    {
        AfterGetServiceExtensions.Add(extension);
    }

    public T GetOptions<T>() where T : ServiceProviderOptions
    {
        var options = _extendedOptions.FirstOrDefault(x => x is T) as T;
        return options;
    }

    public void AddOption(ServiceProviderOptions option)
    {
        _extendedOptions.Add(option);
    }
}