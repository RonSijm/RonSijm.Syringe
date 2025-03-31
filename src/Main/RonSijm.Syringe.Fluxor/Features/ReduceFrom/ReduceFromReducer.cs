using Fluxor;

namespace RonSijm.Syringe;

internal class ReduceFromReducer<TState, TAction> : Reducer<TAction, TState> where TState : new()
{
    public Func<TState, TAction> ReduceAction { get; set; }
    
    public override TAction Reduce(TAction state, TState action)
    {
        if (action is null)
        {
            return default;
        }

        var newState = ReduceAction(action);
        return newState;
    }
}