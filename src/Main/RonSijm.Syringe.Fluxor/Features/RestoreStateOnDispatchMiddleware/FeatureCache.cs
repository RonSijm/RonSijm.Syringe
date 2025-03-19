using Fluxor;

namespace RonSijm.Syringe;

public class FeatureCache
{
    private Dictionary<Type, IFeature> _featureCache = new();
    private IStore _store;

    public IFeature GetFeature(object action)
    {
        var actionType = action.GetType();

        if (_featureCache.TryGetValue(actionType, out var feature))
        {
            return feature;
        }

        var actionFeature = _store.Features.Values.FirstOrDefault(x => x.GetStateType() == actionType);
        _featureCache.Add(actionType, actionFeature);

        return actionFeature;
    }

    public void Initialize(IStore store)
    {
        _store = store;
    }
}