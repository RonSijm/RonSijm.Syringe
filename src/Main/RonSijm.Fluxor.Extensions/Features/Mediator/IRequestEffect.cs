namespace RonSijm.Syringe;

public interface IRequestEffect
{
}

public interface IRequestEffect<T> : IRequestEffect
{
    Task<T> HandleRaw(object request, IDispatcher dispatcher);
}