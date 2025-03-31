using Fluxor;

namespace RonSijm.Syringe;

public static class DispatcherExtensions
{
    public static void Update<TState>(this IDispatcher dispatcher, Action<TState> update) where TState : class, new()
    {
        dispatcher.Dispatch(new Update<TState>(update));
    }

    public static void Dispatch<TState>(this IDispatcher dispatcher, Action<TState> update) where TState : class, new()
    {
        var newState = new TState();
        update(newState);
        dispatcher.Dispatch(newState);
    }
}