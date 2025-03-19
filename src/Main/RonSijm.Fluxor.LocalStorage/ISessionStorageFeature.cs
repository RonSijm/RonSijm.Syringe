using Fluxor;

namespace RonSijm.Syringe;

public interface ILocalStorageFeature : IFeature
{
}

public interface ILocalStorageFeature<T> : ILocalStorageFeature, IFeature<T> where T : new()
{
}