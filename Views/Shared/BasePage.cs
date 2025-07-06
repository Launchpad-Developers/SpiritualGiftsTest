using System.Runtime.Versioning;

namespace SpiritualGiftsTest.Views.Shared;

public partial class BasePage : ContentPage
{
    protected BaseViewModel ViewModel { get; }

    protected BasePage(BaseViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = viewModel;
    }
}


