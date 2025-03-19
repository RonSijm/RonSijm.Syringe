namespace RonSijm.Syringe;
public interface IFailedAction<TRequest> : IFailedAction
{
    public TRequest Request { get; }
}

public interface IFailedAction
{
    public Exception Exception { get; }
}