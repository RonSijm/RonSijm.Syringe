namespace RonSijm.Syringe;

public abstract class FluxorDispatcherComponent : FluxorComponent, IHaveDispatcher
{
    [Inject] public IDispatcher Dispatcher { set; get; }
}