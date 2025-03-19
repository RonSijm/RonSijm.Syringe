using Fluxor;
using System.Reflection;

namespace RonSijm.Syringe.DependencyInjection.InfoFactories;

internal class FeatureStateInfoFactory
{
	internal static FeatureStateInfo[] Create(IEnumerable<Type> allCandidateTypes,
        bool optionsDisableUpdateChildrenFeature)
    {
        return allCandidateTypes
            .Select(x => new
            {
                Type = x,
                FeatureStateAttribute = x.GetCustomAttribute<FeatureStateAttribute>()
            })
            .Where(x => x.FeatureStateAttribute is not null)
            .Select(x => new FeatureStateInfo(x.FeatureStateAttribute, x.Type, optionsDisableUpdateChildrenFeature))
            .ToArray();
    }
}
