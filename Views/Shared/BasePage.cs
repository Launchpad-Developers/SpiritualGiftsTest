using System.Runtime.Versioning;

namespace SpiritualGiftsSurvey.Views.Shared;

public partial class BasePage : ContentPage
{
    protected BaseViewModel ViewModel { get; }

    protected BasePage(BaseViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ViewModel.RefreshViewModel();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        ViewModel.InitAsync();
    }
}


