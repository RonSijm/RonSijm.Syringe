using System.Reflection;
using Fluxor;
using RonSijm.Syringe.DependencyInjection.ServiceRegistration;

namespace RonSijm.Syringe;

internal static class ReduceFromAttributeHandler
{
    internal static void HandleReduceFromAttribute(this PropertyInfo propertyInfo, Type stateType, SyringeServiceProvider serviceProvider)
    {
        var openReducerType = typeof(ReduceFromReducer<,>);
        var reduceFromReducer = openReducerType.MakeGenericType(stateType, propertyInfo.PropertyType);
        var instance = Activator.CreateInstance(reduceFromReducer);

        var reducerActionType = typeof(Func<,>).MakeGenericType(stateType, propertyInfo.PropertyType);
        var reducerAction = CreateReducerFromAction(stateType, propertyInfo);

        var reduceActionProperty = reduceFromReducer.GetProperty(nameof(ReduceFromReducer<object, object>.ReduceAction));
        if (reduceActionProperty is not null && reduceActionProperty.CanWrite)
        {
            reduceActionProperty.SetValue(instance, reducerAction);
        }

        var openFeatureType = typeof(IFeature<>);
        var featureType = openFeatureType.MakeGenericType(propertyInfo.PropertyType);

        var service = serviceProvider.GetService(featureType);
        var featureAddReducerMethodInfo = FeatureRegistration.GetAddReducerMethod(featureType);
        featureAddReducerMethodInfo.Invoke(service, [instance]);
    }

    private static object CreateReducerFromAction(Type stateType, PropertyInfo propertyInfo)
    {
        var method = typeof(ReduceFromHelper)
            .GetMethod(nameof(ReduceFromHelper.CreateTypedReducer), BindingFlags.Static | BindingFlags.Public)!
            .MakeGenericMethod(propertyInfo.PropertyType, stateType);

        return method.Invoke(null, [propertyInfo]);
    }

    public static class ReduceFromHelper
    {
        public static Func<TState, TAction> CreateTypedReducer<TAction, TState>(PropertyInfo propertyInfo)
        {
            var result = new Func<TState, TAction>(action =>
            {
                var value = propertyInfo.GetValue(action);
                return (TAction)value;
            });

            return result;
        }
    }
}