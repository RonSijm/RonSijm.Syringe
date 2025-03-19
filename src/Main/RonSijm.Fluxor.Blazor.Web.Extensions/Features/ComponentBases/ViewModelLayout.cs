namespace RonSijm.Syringe;

public abstract class ViewModelLayout<TViewModel> : FluxorLayout
{
    [Inject] public IState<TViewModel> ViewModelState { set; get; }

    protected TViewModel ViewModel => ViewModelState.Value;
}