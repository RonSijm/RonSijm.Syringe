using Fluxor;

namespace RonSijm.Syringe;

public class UpdateEffect(IServiceProvider serviceProvider) : OpenGenericEffect
{
    protected override Type HandleType => typeof(Update<>);
    public override Task HandleAsync(object action, IDispatcher dispatcher)
    {
        var typeofAction = action.GetType();
        var genericArgument = typeofAction.GetGenericArguments()[0];

        var openFeatureType = typeof(IFeature<>);
        var featureType = openFeatureType.MakeGenericType(genericArgument);
        var feature = serviceProvider.GetService(featureType) as IFeature;

        var currentState = feature.GetState();
        dynamic updateFunction = action;

        var newState = updateFunction.Handle(currentState);
        dispatcher.Dispatch(newState);
        return Task.CompletedTask;
    }
}