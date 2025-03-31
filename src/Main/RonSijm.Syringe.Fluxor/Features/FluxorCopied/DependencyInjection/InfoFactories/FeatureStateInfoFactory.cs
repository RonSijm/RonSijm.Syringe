using System.Reflection;
using Fluxor;

namespace RonSijm.Syringe.DependencyInjection.InfoFactories;

internal class FeatureStateInfoFactory
{
	internal static List<FeatureStateInfo> Create(Type[] allCandidateTypes,
        bool optionsDisableUpdateChildrenFeature)
    {
        var featureStateInfoByAttribute = allCandidateTypes
            .Select(x => new
            {
                Type = x,
                FeatureStateAttribute = x.GetCustomAttribute<FeatureStateAttribute>()
            })
            .Where(x => x.FeatureStateAttribute is not null)
            .Select(x => new FeatureStateInfo(x.FeatureStateAttribute, x.Type, optionsDisableUpdateChildrenFeature))
            .ToList();

        var featureStateInfoByInterface = allCandidateTypes
            .Select(x => new
            {
                Type = x,
                HasInterface = x.IsAssignableTo(typeof(IFeatureState))
            })
            .Where(x => x.HasInterface)
            .Select(x =>
            {
                var attribute = new FeatureStateAttribute();
                return new FeatureStateInfo(attribute, x.Type, optionsDisableUpdateChildrenFeature);
            })
            .ToList();

        featureStateInfoByAttribute.AddRange(featureStateInfoByInterface);

        return featureStateInfoByAttribute;
    }
}
