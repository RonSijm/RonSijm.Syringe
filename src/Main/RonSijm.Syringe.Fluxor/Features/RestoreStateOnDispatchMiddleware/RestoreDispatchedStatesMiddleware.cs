using Fluxor;

namespace RonSijm.Syringe;

public class RestoreDispatchedStatesMiddleware(FeatureCache featureCache) : Middleware
{
    public override Task InitializeAsync(IDispatcher dispatcher, IStore store)
    {
        featureCache.Initialize(store);
        return Task.CompletedTask;
    }

    public override void AfterDispatch(object action)
    {
        var feature = featureCache.GetFeature(action);

        if (feature == null)
        {
            return;
        }

        var currentState = feature.GetState();

        if (action == null && currentState == null)
        {
            return;
        }

        if (action == null || currentState == null)
        {
            feature.RestoreState(action);
            return;
        }

        if (action.Equals(currentState))
        {
            return;
        }

        feature.RestoreState(action);
    }
}