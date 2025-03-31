using Fluxor;

namespace RonSijm.Syringe;

public abstract class OpenGenericEffect : IEffect
{
    protected abstract Type HandleType { get; }

    public abstract Task HandleAsync(object action, IDispatcher dispatcher);

    public bool ShouldReactToAction(object action)
    {
        if (action == null)
        {
            return false;
        }

        var actionType = action.GetType();
        if (!actionType.IsGenericType)
        {
            return false;
        }

        var genericType = actionType.GetGenericTypeDefinition();
        var isCorrectType = genericType.IsAssignableTo(HandleType);
        return isCorrectType;
    }
}