namespace RonSijm.Syringe;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class EffectWhenProp<TTriggerAction> : EffectWhen<TTriggerAction>
{
    protected abstract Func<TTriggerAction, bool> When { get; }

    protected override bool ShouldReactToAction(TTriggerAction action)
    {
        return When(action);
    }
}

public abstract class EffectWhen<TTriggerAction> : IEffect
{
    protected abstract bool ShouldReactToAction(TTriggerAction action);

    protected abstract Task HandleAsync(TTriggerAction action, IDispatcher dispatcher);

    public bool ShouldReactToAction(object action)
    {
        if (action is not TTriggerAction typedAction)
        {
            return false;
        }

        return ShouldReactToAction(typedAction);
    }

    Task IEffect.HandleAsync(object action, IDispatcher dispatcher)
    {
        return HandleAsync((TTriggerAction)action, dispatcher);
    }
}