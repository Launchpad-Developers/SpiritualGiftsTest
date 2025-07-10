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
        Title = "Spiritual Gifts Survey";
    }

    public ObservableCollection<QuestionViewModel> Questions { get; set; } = new();

    [ObservableProperty]
    private string tapTo = string.Empty;

    [ObservableProperty]
    private string navigate = string.Empty;

    [ObservableProperty]
    private string nextPage = string.Empty;

    [RelayCommand]
    private async Task OpenInfoAsync()
    {
        if (!IsLoading)
            await PerformNavigation(Routes.AppInfoPage);
    }

    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        if (!IsLoading)
        {
            await PerformNavigation(Routes.SettingsPage);
            return;
        }
    }

    public override async void InitAsync()
    {
        if (!RequiresInitialzation)
            return;

        RequiresInitialzation = false;

        IsLoading = true;

        await TranslationService.InitializeLanguage();

        if (Questions.Count > 0)
        {
            Questions.RemoveAt(0);
        };

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
