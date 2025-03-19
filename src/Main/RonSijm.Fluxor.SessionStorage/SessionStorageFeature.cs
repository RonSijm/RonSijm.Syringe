using Blazored.SessionStorage;
using Fluxor;

namespace RonSijm.Syringe;

public abstract class SessionStorageFeature<T> : Feature<T>, ISessionStorageFeature<T> where T : new()
{
    private readonly ISyncSessionStorageService _sessionStorageService;
    private readonly bool _newWhenNull;

    public SessionStorageFeature(ISyncSessionStorageService sessionStorageService, bool newWhenNull = false)
    {
        _newWhenNull = newWhenNull;
        _sessionStorageService = sessionStorageService;
        StateChanged += OnStateChanged;
    }

    private void OnStateChanged(object sender, EventArgs e)
    {
        _sessionStorageService?.SetItem(Name, e);
    }

    protected abstract string Name { get; }

    public override string GetName()
    {
        return Name;
    }

    protected override T GetInitialState()
    {
        if (_sessionStorageService == null)
        {
            return default;
        }

        var state = _sessionStorageService.GetItem<T>(Name);

        if (state == null && _newWhenNull)
        {
            return new();
        }

        return state;
    }
}