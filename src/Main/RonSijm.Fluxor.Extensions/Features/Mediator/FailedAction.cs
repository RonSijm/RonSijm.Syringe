namespace RonSijm.Syringe;

[FeatureState]
public abstract record FailedAction<TRequest>(TRequest Request, Exception Exception) : IFailedAction<TRequest>;