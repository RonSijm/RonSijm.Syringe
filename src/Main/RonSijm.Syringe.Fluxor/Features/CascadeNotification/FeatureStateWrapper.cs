using Fluxor;
using RonSijm.Syringe.DependencyInjection;

namespace RonSijm.Syringe;

internal class UpdateChildrenFeatureStateWrapper<TState> : UpdateChildrenFeature<TState>
{
	private readonly string Name;
	private readonly Func<object> CreateInitialStateFunc;

	public UpdateChildrenFeatureStateWrapper(FeatureStateInfo info, IDispatcher dispatcher) : base(dispatcher)
    {
		Name = info.FeatureStateAttribute.Name ?? typeof(TState).FullName;
		MaximumStateChangedNotificationsPerSecond = info.FeatureStateAttribute.MaximumStateChangedNotificationsPerSecond;
		CreateInitialStateFunc = info.CreateInitialStateFunc;
	}

	public override string GetName() => Name;

	protected override TState GetInitialState() => (TState)CreateInitialStateFunc();
}
