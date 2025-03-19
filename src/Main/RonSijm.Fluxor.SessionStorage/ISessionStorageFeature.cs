using Fluxor;

namespace RonSijm.Syringe;

public interface ISessionStorageFeature : IFeature
{
}

public interface ISessionStorageFeature<T> : ISessionStorageFeature, IFeature<T> where T : new()
{
}