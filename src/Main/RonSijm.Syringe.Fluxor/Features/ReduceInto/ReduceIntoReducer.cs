using Fluxor;

namespace RonSijm.Syringe;

internal class ReduceIntoReducer<TState, TAction> : Reducer<TState, TAction> where TState : new()
{
    public Action<TState, TAction> ReduceAction { get; set; }

    public override TState Reduce(TState state, TAction action)
    {
        var newState = new TState();

        if (state is null)
        {
            ReduceAction(newState, action);
            return newState;
        }

        state.CopyProperties(newState);

        ReduceAction(newState, action);
        return newState;
    }
}