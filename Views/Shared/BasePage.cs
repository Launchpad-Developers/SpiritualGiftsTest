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

        // HIGH-3 FIX: RefreshViewModel is synchronous, safe to call here
        ViewModel.RefreshViewModel();
    }

    // HIGH-3 FIX: OnNavigatedTo calls coordinated InitializeAsync instead of fire-and-forget InitAsync
    // This prevents overlapping initialization and ensures exceptions are observable
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        // Dispatch async initialization with proper await pattern
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await ViewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                // Log navigation initialization failure
                System.Diagnostics.Debug.WriteLine($"OnNavigatedTo InitializeAsync failed: {ex.Message}");
            }
        });
    }
}

