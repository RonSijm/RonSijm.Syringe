using System.Reflection;
using Fluxor;
using RonSijm.Syringe.DependencyInjection.ServiceRegistration;

namespace RonSijm.Syringe;

internal static class ReduceIntoAttributeHandler
{
    public static void HandleReduceIntoAttribute(this PropertyInfo propertyInfo, Type stateType, IFeature service)
    {
        var openReducerType = typeof(ReduceIntoReducer<,>);
        var reduceIntoReducer = openReducerType.MakeGenericType(stateType, propertyInfo.PropertyType);
        var instance = Activator.CreateInstance(reduceIntoReducer);

        var reducerActionType = typeof(Action<,>).MakeGenericType(stateType, propertyInfo.PropertyType);
        var reducerAction = CreateReducerIntoAction(stateType, propertyInfo);

        var reduceActionProperty = reduceIntoReducer.GetProperty(nameof(ReduceIntoReducer<object, object>.ReduceAction));
        if (reduceActionProperty is not null && reduceActionProperty.CanWrite)
        {
            reduceActionProperty.SetValue(instance, reducerAction);
        }

        var openFeatureType = typeof(IFeature<>);
        var featureType = openFeatureType.MakeGenericType(stateType);

        var featureAddReducerMethodInfo = FeatureRegistration.GetAddReducerMethod(featureType);
        featureAddReducerMethodInfo.Invoke(service, [instance]);
    }


    private static object CreateReducerIntoAction(Type stateType, PropertyInfo propertyInfo)
    {
        var method = typeof(ReduceIntoHelper)
            .GetMethod(nameof(ReduceIntoHelper.CreateTypedReducer), BindingFlags.Static | BindingFlags.Public)!
            .MakeGenericMethod(stateType, propertyInfo.PropertyType);

        return method.Invoke(null, [propertyInfo]);
    }

    public static class ReduceIntoHelper
    {
        public static Action<TState, TAction> CreateTypedReducer<TState, TAction>(PropertyInfo propertyInfo)
        {
            return (state, action) =>
            {
                if (state is not null && action is not null)
                {
                    propertyInfo.SetValue(state, action);
                }
            };
        }
    }
}