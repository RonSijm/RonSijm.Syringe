namespace RonSijm.Syringe;

public abstract class CompleteEffect<TState, TAction> : EffectWithResult<TAction, TState> where TAction : ICompleteAction<TState>
{
    protected override async Task<TState> HandleAsync(TAction action)
    {
        return action.Response;
    }
}