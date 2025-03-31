using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.DependencyInjection.InfoFactories;
using RonSijm.Syringe.DependencyInjection.ServiceRegistration;

namespace RonSijm.Syringe.DependencyInjection;

internal static class ReflectionScanner
{
	internal static void Scan(this IServiceCollection services, SyringeFluxorOptions options, IEnumerable<Type> typesToScan, IEnumerable<AssemblyScanSettings> assembliesToScan, IEnumerable<AssemblyScanSettings> scanIncludeList)
	{
		var totalScanSources = 0;
		totalScanSources += assembliesToScan?.Count() ?? 0;
		totalScanSources += typesToScan?.Count() ?? 0;

		if (totalScanSources < 1)
			throw new ArgumentException($"Must supply either {typesToScan} or {assembliesToScan}");

		GetCandidateTypes(
			assembliesToScan: assembliesToScan,
			typesToScan: typesToScan,
			scanIncludeList: scanIncludeList ?? new List<AssemblyScanSettings>(),
			allCandidateTypes: out var allCandidateTypes,
			allNonAbstractCandidateTypes: out var allNonAbstractCandidateTypes);

		// Method reducer/effects may belong to abstract classes
		var allCandidateMethods =
			AssemblyScanSettings.FilterMethods(allCandidateTypes);

		// Find all concrete implementors of IReducer<T>
		var reducerClassInfos =
			ReducerClassInfoFactory.Create(services, allNonAbstractCandidateTypes);

		// Find all [ReducerMethod] decorated methods
		var reducerMethodInfos =
			ReducerMethodInfoFactory.Create(allCandidateMethods);

		// Find all concrete implementors of IEffect<T>
		var effectClassInfos =
			EffectClassInfoFactory.Create(services, allNonAbstractCandidateTypes);

		// Find all [EffectMethod] decorated methods
		var effectMethodInfos =
			EffectMethodInfoFactory.Create(services, allCandidateMethods);

		// Find all concrete implementors of IFeature
		var featureClassInfos =
			FeatureClassInfoFactory.Create(
				services: services,
				allCandidateTypes: allNonAbstractCandidateTypes,
				reducerClassInfos: reducerClassInfos,
				reducerMethodInfos: reducerMethodInfos);

		var featureStateInfos = FeatureStateInfoFactory.Create(allCandidateTypes: allCandidateTypes, options.DisableUpdateChildrenFeature);

		StoreRegistration.Register(
			services,
			options,
			featureClassInfos,
			featureStateInfos,
			reducerClassInfos,
			reducerMethodInfos,
			effectClassInfos,
			effectMethodInfos);
	}

	private static void GetCandidateTypes(
		IEnumerable<AssemblyScanSettings> assembliesToScan,
		IEnumerable<Type> typesToScan,
		IEnumerable<AssemblyScanSettings> scanIncludeList,
		out Type[] allCandidateTypes,
		out Type[] allNonAbstractCandidateTypes)
	{
		var allCandidateAssemblies =
			assembliesToScan
				.Select(x => x.Assembly)
				.Union(scanIncludeList.Select(x => x.Assembly))
				.Distinct()
				.ToArray();

		var scanExcludeList =
			MiddlewareClassesDiscovery.FindMiddlewareLocations(allCandidateAssemblies);

		allCandidateTypes = AssemblyScanSettings.FilterClasses(
			scanExcludeList: scanExcludeList,
			scanIncludeList: scanIncludeList,
			types:
				allCandidateAssemblies
					.SelectMany(x => x.GetTypes())
					.Union(scanIncludeList.SelectMany(x => x.Assembly.GetTypes()))
					.Where(t => !t.IsGenericTypeDefinition)
					.Distinct()
					.ToArray()
			)
			.Union(typesToScan)
			.ToArray();
		allNonAbstractCandidateTypes = allCandidateTypes
				.Where(t => !t.IsAbstract)
				.ToArray();
	}
}
