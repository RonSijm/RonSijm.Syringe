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
        feature?.RestoreState(action);
    }
}