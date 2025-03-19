namespace RonSijm.Syringe;

public abstract class EffectWithResult<T, TResult> : Effect<T>
{
    protected IDispatcher Dispatcher;
    
    public override async Task HandleAsync(T action, IDispatcher dispatcher)
    {
        Dispatcher = dispatcher;

        var result = await HandleAsync(action);

        if (result != null)
        {
            dispatcher.Dispatch(result);
        }
    }

    protected abstract Task<TResult> HandleAsync(T action);
}