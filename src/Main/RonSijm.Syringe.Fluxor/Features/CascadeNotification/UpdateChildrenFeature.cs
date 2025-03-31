using Fluxor;

namespace RonSijm.Syringe;

public abstract class UpdateChildrenFeature<TState> : Feature<TState>
{
    private TState _previousState;

    public UpdateChildrenFeature(IDispatcher dispatcher)
    {
        StateChanged += (sender, args) => { Update(dispatcher, sender, args); };
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