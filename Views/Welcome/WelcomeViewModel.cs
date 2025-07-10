using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SpiritualGiftsSurvey.Messages;
using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Routing;
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

        Title = "Spiritual Gifts Survey";

        WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (r, m) =>
        {
            InitializePageAsync();
        });
        
        Languages = new ObservableCollection<string>();

        //No translation data available before this point
        InitializePageAsync();
    }

    public ObservableCollection<QuestionViewModel> Questions { get; set; } = new();

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

    //[RelayCommand]
    //private async Task GetStartedAsync()
    //{
    //    ShowInstructable = !ShowInstructable;

    //    if (!ShowInstructable)
    //        await PerformNavigation(Routes.SurveyPage);
    //}

    [RelayCommand]
    private async Task OpenInfoAsync()
    {
        if (!IsLoading)
            await PerformNavigation(Routes.AppInfoPage);
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
            await PerformNavigation(Routes.SettingsPage);
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

        string instructions = string.Format(TranslationService.GetString("Instructions", "Instructions"), Environment.NewLine);
        Questions.Add(new QuestionViewModel { QuestionText = instructions });

        FlowDirection = TranslationService.FlowDirection;
        LoadingText = TranslationService.GetString("Loading", "Loading");
        PageTopic = TranslationService.GetString("AppTitle", "Spiritual Gift Survey");
        NavButtonText = TranslationService.GetString("Begin", "Begin");
        TapTo = TranslationService.GetString("ScrollTo", "Scroll to");
        Navigate = TranslationService.GetString("Navigate", "navigate");
        NextPage = TranslationService.GetString("SurveyPage", "SurveyPage");

        IsLoading = false;
    }

}
