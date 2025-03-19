namespace RonSijm.Syringe;

public abstract class TypeNameFeature<TState> : Feature<TState>
{
    public override string GetName()
    {
        return nameof(TState);
    }
}