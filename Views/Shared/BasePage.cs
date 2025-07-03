namespace SpiritualGiftsTest.Views.Shared;

public abstract class BasePage<TViewModel> : ContentPage
    where TViewModel : BaseViewModel
{
    protected TViewModel ViewModel { get; }

    protected BasePage(TViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = viewModel;
    }
}

