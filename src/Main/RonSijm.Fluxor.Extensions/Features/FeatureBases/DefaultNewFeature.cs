namespace RonSijm.Syringe;

public abstract class DefaultNewFeature<TState> : TypeNameFeature<TState> where TState : new()
{
    public override string GetName()
    {
        return nameof(TState);
    }

    protected override TState GetInitialState()
    {
        return new();
    }
}