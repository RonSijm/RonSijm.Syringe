using System.Reflection;
using Fluxor;

namespace RonSijm.Syringe.DependencyInjection.InfoFactories;

internal static class ReducerMethodInfoFactory
{
	internal static ReducerMethodInfo[] Create(IEnumerable<TypeAndMethodInfo> allCandidateMethods)
	=>
		allCandidateMethods
			.Select(c =>
				new
				{
					HostClassType = c.Type, 
					c.MethodInfo,
					ReducerAttribute = c.MethodInfo.GetCustomAttribute<ReducerMethodAttribute>(false)
				})
			.Where(x => x.ReducerAttribute is not null)
			.Select(x => new ReducerMethodInfo(
				x.HostClassType,
				x.ReducerAttribute,
				x.MethodInfo))
			.ToArray();
}
