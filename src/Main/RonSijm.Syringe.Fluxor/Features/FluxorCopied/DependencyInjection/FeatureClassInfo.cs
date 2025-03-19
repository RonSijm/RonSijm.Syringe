using Fluxor;

namespace RonSijm.Syringe.DependencyInjection;

internal class FeatureClassInfo
{
	public readonly Type FeatureInterfaceGenericType;
	public readonly Type ImplementingType;
	public readonly Type StateType;

	public FeatureClassInfo(Type implementingType, Type stateType)
	{
		FeatureInterfaceGenericType = typeof(IFeature<>).MakeGenericType(stateType);
		ImplementingType = implementingType;
		StateType = stateType;
	}
}
