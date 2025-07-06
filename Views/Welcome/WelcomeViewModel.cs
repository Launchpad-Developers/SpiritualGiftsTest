using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SpiritualGiftsTest.Helpers;
using SpiritualGiftsTest.Messages;
using SpiritualGiftsTest.Models;
using SpiritualGiftsTest.Services;
using SpiritualGiftsTest.Views.Shared;
using System.Collections.ObjectModel;

namespace SpiritualGiftsTest.Views.Welcome;


public partial class WelcomeViewModel : BaseViewModel
{
    private bool _databaseOK;

    public WelcomeViewModel(
        IAggregatedServices aggregatedServices, 
        IPreferences preferences) : base(aggregatedServices, preferences)
    {
        Languages = new ObservableCollection<string>();

        Title = "Welcome to the Spiritual Gifts Survey";

        WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (r, m) =>
        {
            _databaseOK = false;
            InitializePageAsync();
        });

        //Question = new Tuple<string, string>(string.Empty, string.Empty);
        Languages = new ObservableCollection<string>();

        InitializePageAsync();
    }

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private string tapTo = string.Empty;

    [ObservableProperty]
    private string navigate = string.Empty;

    [ObservableProperty]
    private string nextPage = string.Empty;

    [ObservableProperty]
    private QuestionViewModel question = new();

    [ObservableProperty]
    private ObservableCollection<string> languages = new();

    [RelayCommand]
    private void GetStarted()
    {
        // TODO: navigate to your next page, e.g.:
        // await Application.Current.MainPage.Navigation.PushAsync(
        //     new ContentPage() { Title = "Next" });
    }

    [RelayCommand]
    private async Task OpenInfoAsync()
    {
        if (!IsLoading)
            await NavigationService.NavigateAsync("AppInfo");
    }

    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        if (ErrorVisible)
        {
            await NotifyUserAsync("Network Error", "Your device is not connected to the internet. Language updates are not currently available.", "OK");
            return;
        }

        if (!IsLoading)
        {
            var result = await NavigationService.NavigateAsync("Settings");
            return;
        }
    }

    [RelayCommand]
    private void RetryUpdate()
    {
        InitializePageAsync();
    }

    public async void InitializePageAsync()
    {
        if (_databaseOK) return;

        LoadingText = "Loading";
        IsLoading = true;
        ErrorVisible = false;
        OnPropertyChanged(nameof(ShowControls));

        await TranslationService.InitializeLanguage();

        var lang = TranslationService.Language;

        if (lang == null)
        {
            PageTopic = "Spiritual Gifts Survey";
            ErrorMessage = "No Internet Connection \nNo Translations Available";
            NavButtonText = "Retry";
            ErrorVisible = true;
        }
        else
        {
            FlowDirection = lang.FlowDirection.Equals("RTL") ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            LoadingText = CurrentTranslation.AppStrings.Get("Loading", "Loading");
            PageTopic = CurrentTranslation.AppStrings.Get("AppTitle", "Spiritual Gifts Survey");
            NavButtonText = CurrentTranslation.AppStrings.Get("NavButtonBegin", "Begin");
            _databaseOK = true;
            ConfirmButtonText = CurrentTranslation.AppStrings.Get("GotIt", "Got it");
            TapTo = CurrentTranslation.AppStrings.Get("TapArrows", "Tap arrows");
            Navigate = CurrentTranslation.AppStrings.Get("ToNavigate", "to navigate");
        }

        NextPage = "SurveyPage";

        IsLoading = false;
        ShowInstructable = true;
    }

}
