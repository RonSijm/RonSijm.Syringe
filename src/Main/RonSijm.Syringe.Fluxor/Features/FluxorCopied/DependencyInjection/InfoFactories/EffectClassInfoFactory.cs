using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.DependencyInjection.Wrappers;
using Fluxor;

namespace RonSijm.Syringe.DependencyInjection.InfoFactories;

internal static class EffectClassInfoFactory
{
	internal static EffectClassInfo[] Create(
		IServiceCollection services,
		IEnumerable<Type> allCandidateTypes)
	=>
		allCandidateTypes
			.Where(t => typeof(IEffect).IsAssignableFrom(t))
			.Where(t => t != typeof(EffectWrapper<>))
			.Select(t => new EffectClassInfo(implementingType: t))
			.ToArray();
}
