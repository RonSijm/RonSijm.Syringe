using Fluxor;
using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe
{
    public class WireFluxorAfterBuildExtension : ISyringeAfterBuildExtension
    {
        private SyringeServiceProvider ServiceProvider;

        public void SetReference(SyringeServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public void Process(List<ServiceDescriptor> loadedDescriptors, bool isInitialBuild)
        {
            if (isInitialBuild || loadedDescriptors == null || !loadedDescriptors.Any())
            {
                return;
            }

            var otherTypes = new List<ServiceDescriptor>();
            var store = ServiceProvider.GetService<IStore>();

            foreach (var serviceDescriptor in loadedDescriptors)
            {
                if (serviceDescriptor.ServiceType.IsAssignableTo(typeof(IFeature)))
                {
                    var service = ServiceProvider.GetService(serviceDescriptor.ServiceType) as IFeature;
                    store.AddFeature(service);
                }
                else if (serviceDescriptor.ServiceType.IsAssignableTo(typeof(IEffect)))
                {
                    var service = ServiceProvider.GetService(serviceDescriptor.ServiceType) as IEffect;
                    store.AddEffect(service);
                }
                else if (serviceDescriptor.ServiceType.IsAssignableTo(typeof(IMiddleware)))
                {
                    var service = ServiceProvider.GetService(serviceDescriptor.ServiceType) as IMiddleware;
                    store.AddMiddleware(service);
                }
                else
                {
                    otherTypes.Add(serviceDescriptor);
                }
            }

            var types = otherTypes.Select(x => x.ServiceType).ToList();
            var filter = AssemblyScanSettings.FilterMethods(types);
            var reducerMethods = ReducerMethodInfoFactory.Create(filter);

            foreach (var reducerMethodInfo in reducerMethods)
            {
                var reducerInstance = ReducerWrapperFactory.Create(ServiceProvider, reducerMethodInfo);

                var openFeatureType = typeof(IFeature<>);
                var featureType = openFeatureType.MakeGenericType(reducerMethodInfo.StateType);
                var feature = ServiceProvider.GetService(featureType) as IFeature;
                var featureAddReducerMethodInfo = GetAddReducerMethodHelper.GetAddReducerMethod(featureType);
                featureAddReducerMethodInfo.Invoke(feature, [reducerInstance]);
            }
        }
    }
}
