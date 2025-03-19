using Blazored.LocalStorage;
using Fluxor;

namespace RonSijm.Syringe;

public abstract class LocalStorageFeature<T> : Feature<T>, ILocalStorageFeature<T> where T : new()
{
    private readonly ISyncLocalStorageService _localStorageService;
    private readonly bool _newWhenNull;

    public LocalStorageFeature(ISyncLocalStorageService localStorageService, bool newWhenNull = false)
    {
        _newWhenNull = newWhenNull;
        _localStorageService = localStorageService;
        StateChanged += OnStateChanged;
    }

    private void OnStateChanged(object sender, EventArgs e)
    {
        _localStorageService?.SetItem(Name, e);
    }
    
    protected abstract string Name { get; }

    public override string GetName()
    {
        return Name;
    }

    protected override T GetInitialState()
    {
        if (_localStorageService == null)
        {
            return default;
        }

        var state = _localStorageService.GetItem<T>(Name);

        if (state == null && _newWhenNull)
        {
            return new();
        }

        return state;
    }
}