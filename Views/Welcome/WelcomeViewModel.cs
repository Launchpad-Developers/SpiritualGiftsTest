using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SpiritualGiftsSurvey.Messages;
using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Views.Shared;
using System.Collections.ObjectModel;

namespace SpiritualGiftsSurvey.Views.Welcome;


public partial class WelcomeViewModel : BaseViewModel
{
    public WelcomeViewModel(
        IAggregatedServices aggregatedServices, 
        IPreferences preferences) : base(aggregatedServices, preferences)
    {
        Languages = new ObservableCollection<string>();

        Title = "Welcome to the Spiritual Gifts Survey";

        WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (r, m) =>
        {
            InitializePageAsync();
        });

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
    private async Task GetStartedAsync()
    {
        if (!IsLoading)
            await NavigationService.NavigateAsync("Settings");
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
        IsLoading = true;
        ErrorVisible = false;

        await TranslationService.InitializeLanguage();

        FlowDirection = TranslationService.FlowDirection;
        LoadingText = TranslationService.GetString("Loading", "Loading");
        PageTopic = TranslationService.GetString("AppTitle", "Spiritual Gift Survey_");
        NavButtonText = TranslationService.GetString("NavButtonBegin", "Begin_");
        ConfirmButtonText = TranslationService.GetString("GotIt", "Got it_");
        TapTo = TranslationService.GetString("TapArrows", "Tap arrows_");
        Navigate = TranslationService.GetString("ToNavigate", "to navigate_");
        NextPage = TranslationService.GetString("SurveyPage", "SurveyPage");

        IsLoading = false;
        ShowInstructable = true;
    }

}
