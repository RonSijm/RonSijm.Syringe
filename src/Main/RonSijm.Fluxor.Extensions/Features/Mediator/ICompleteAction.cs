namespace RonSijm.Syringe;

public interface ICompleteAction;

public interface ICompleteAction<out TResponse> : ICompleteAction
{
    public TResponse Response { get; }
    public Exception Exception { get; set; }
}

public interface ICompleteAction<out TRequest, out TResponse> : ICompleteAction<TResponse>
{
    public TRequest Request { get; }
}