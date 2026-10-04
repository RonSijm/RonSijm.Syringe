using Fluxor;

namespace RonSijm.Syringe;

public abstract class UpdateChildrenFeature<TState> : Feature<TState>, IDispatchedStateFeature
{
    private TState _previousState;

    public UpdateChildrenFeature(IDispatcher dispatcher)
    {
        StateChanged += (sender, args) => { Update(dispatcher, sender, args); };
    }

    void IDispatchedStateFeature.RestoreDispatchedState(object state)
    {
        // This snapshot is already an action; restoring it must not dispatch it again.
        _previousState = (TState)state;
        ((IFeature)this).RestoreState(state);
    }

    private void Update(IDispatcher dispatcher, object sender, EventArgs args)
    {
        if(State == null)
        {
            _previousState = State;
            return;
        }

        if (State.Equals(_previousState))
        {
            return;
        }

        if (_previousState != null)
        {
            var areTheSame = CopyPropertiesHelper.CompareObject(typeof(TState), _previousState, State);

            if (areTheSame)
            {
                return;
            }
        }

        _previousState = State;
        dispatcher.Dispatch(State);
    }
}