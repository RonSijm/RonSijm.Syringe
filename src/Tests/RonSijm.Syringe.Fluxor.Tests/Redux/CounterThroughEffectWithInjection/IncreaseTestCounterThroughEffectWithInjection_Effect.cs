using Fluxor;
using RonSijm.Syringe.Fluxor.Tests.Redux.ThroughEffectWithInjection;

namespace RonSijm.Syringe.Fluxor.Tests.Redux.CounterThroughEffectWithInjection;

public class InjectAttribute : Attribute
{
}

public class IncreaseTestCounterThroughEffectWithInjection_Child : IncreaseTestCounterThroughEffectWithInjection_Effect
{
}

public abstract class IncreaseTestCounterThroughEffectWithInjection_Effect : Effect<IncreaseTestCounterThroughEffectWithInjection_Action>
{
    [Inject]
    public IState<IncreaseTestCounterThroughEffectWithInjection_State> State { get; set; }

    public override async Task HandleAsync(IncreaseTestCounterThroughEffectWithInjection_Action action, IDispatcher dispatcher)
    {
        int count;

        if (State.Value == null)
        {
            count = -1;
        }
        else
        {
            count = State.Value.Count + 1;
        }

        dispatcher.Update<IncreaseTestCounterThroughEffectWithInjection_State>(x => x.Count = count);
    }
}