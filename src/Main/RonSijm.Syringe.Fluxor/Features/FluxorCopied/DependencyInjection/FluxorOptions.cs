using System.Reflection;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.Extensions;

namespace RonSijm.Syringe.DependencyInjection;

/// <summary>
/// An options class for configuring Fluxor
/// </summary>
public class FluxorOptions
{
    internal List<AssemblyScanSettings> AssembliesToScan { get; private set; } = new();
    internal List<Type> TypesToScan { get; private set; } = new();
    internal List<Type> MiddlewareTypes = new();
	internal StoreLifetime StoreLifetime { get; set; } = StoreLifetime.Scoped;

	/// <summary>
	/// Service collection for registering services
	/// </summary>
	public readonly IServiceCollection Services;

	/// <summary>
	/// Creates a new instance
	/// </summary>
	/// <param name="services"></param>
	public FluxorOptions(IServiceCollection services)
	{
		Services = services;
	}

	public FluxorOptions ScanTypes(Type typeToScan, params Type[] additionalTypesToScan)
	{
        if (typeToScan is null)
        {
            throw new ArgumentNullException(nameof(typeToScan));
        }

		var allTypes = new List<Type> { typeToScan };
        if (additionalTypesToScan is not null)
        {
            allTypes.AddRange(additionalTypesToScan);
        }

		var genericTypeNames = string.Join(",", allTypes.Where(x => x.IsGenericTypeDefinition).Select(x => x.Name));

        if (genericTypeNames != string.Empty)
        {
            throw new InvalidOperationException($"The following types cannot be generic: {genericTypeNames}");
        }

		TypesToScan = TypesToScan.Union(allTypes).ToList();

		return this;
	}

	/// <summary>
	/// The Store Lifetime that should be used when registering Fluxor features/reducers/effects/middleware
	/// </summary>
	/// <param name="lifecycle">the lifecycle to use</param>
	/// <returns>Options</returns>
	/// <remarks>
	/// <list type="bullet">
	/// <item>
	/// <term>LifecycleEnum.Scoped</term>
	/// <description>(default) Create a new instance for each new request</description>
	/// </item>
	/// <item>
	/// <term>LifecycleEnum.Singleton</term>
	/// <description>Create a new instance on first request and reuse for rest of application lifetime</description>
	/// <para>
	/// NOTE: indicating Singleton should be done only for exceptional cases.
	/// For example, in MAUI/Blazor hybrid applications, the main MAUI application is a different scope then each BlazorWebView component
	/// and state needs to be shared across all scopes of the application
	/// </para>
	/// <para>
	/// This value should only be set once during the configuration of Fluxor
	/// </para>
	/// </remarks>
	public FluxorOptions WithLifetime(StoreLifetime lifecycle)
	{
		StoreLifetime = lifecycle;
		return this;
	}

	/// <summary>
	/// Enables automatic discovery of features/effects/reducers
	/// </summary>
	/// <param name="additionalAssembliesToScan">A collection of assemblies to scan</param>
	/// <returns>Options</returns>
	public FluxorOptions ScanAssemblies(
		Assembly assemblyToScan,
		params Assembly[] additionalAssembliesToScan)
	{
		if (assemblyToScan is null)
			throw new ArgumentNullException(nameof(assemblyToScan));

		var allAssemblies = new List<Assembly> { assemblyToScan };
		if (additionalAssembliesToScan is not null)
			allAssemblies.AddRange(additionalAssembliesToScan);

		var newAssembliesToScan = allAssemblies.Select(x => new AssemblyScanSettings(x)).ToList();
		newAssembliesToScan.AddRange(AssembliesToScan);
		AssembliesToScan = newAssembliesToScan.ToList();

		return this;
	}

	/// <summary>
	/// Enables the developer to specify a class that implements <see cref="IMiddleware"/>
	/// which should be injected into the <see cref="IStore.AddMiddleware(IMiddleware)"/> method
	/// after dependency injection has completed.
	/// </summary>
	/// <typeparam name="TMiddleware">The Middleware type</typeparam>
	/// <returns>Options</returns>
	public FluxorOptions AddMiddleware<TMiddleware>() where TMiddleware : IMiddleware
	{
        if (MiddlewareTypes.Contains(typeof(TMiddleware)))
        {
            return this;
        }

        Services.Add(typeof(TMiddleware), this);
		var assembly = typeof(TMiddleware).Assembly;
		var @namespace = typeof(TMiddleware).Namespace;

        AssembliesToScan = new List<AssemblyScanSettings>(AssembliesToScan)
        {
            new AssemblyScanSettings(assembly, @namespace)
        };

        MiddlewareTypes = new List<Type>(MiddlewareTypes)
        {
            typeof(TMiddleware)
        };

		return this;
	}
}
