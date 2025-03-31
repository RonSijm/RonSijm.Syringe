using System.Reflection;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;

namespace RonSijm.Syringe.DependencyInjection.InfoFactories;

internal static class EffectMethodInfoFactory
{
	internal static EffectMethodInfo[] Create(
		IServiceCollection services,
		IEnumerable<TypeAndMethodInfo> allCandidateMethods)
	=>
		allCandidateMethods
			.Select(c =>
				new
				{
					HostClassType = c.Type,
					c.MethodInfo,
					EffectAttribute = c.MethodInfo.GetCustomAttribute<EffectMethodAttribute>(false)
				})
			.Where(x => x.EffectAttribute is not null)
			.Select(x =>
				new EffectMethodInfo(
					x.HostClassType,
					x.EffectAttribute, 
					x.MethodInfo))
			.ToArray();
}
