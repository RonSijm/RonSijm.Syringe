namespace RonSijm.Syringe;

public abstract class ViewModelComponent<TViewModel> : FluxorDispatcherComponent
{
    [Inject] public IState<TViewModel> ViewModelState { set; get; }

    protected TViewModel ViewModel => ViewModelState.Value;
}