namespace RonSijm.Syringe;

public abstract class EffectWhenByCon<TTriggerAction>(Func<TTriggerAction, bool> when) : EffectWhen<TTriggerAction>
{
    protected override bool ShouldReactToAction(TTriggerAction action)
    {
        return when(action);
    }
}