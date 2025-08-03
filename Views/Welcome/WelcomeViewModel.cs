using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpiritualGiftsSurvey.Routing;
using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Views.Controls;
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

    [ObservableProperty] private string tapTo = string.Empty;
    [ObservableProperty] private string navigate = string.Empty;
    [ObservableProperty] private string beginText = string.Empty;
    [ObservableProperty] private string confirmText = string.Empty;

    [RelayCommand]
    private void OnBegin()
    {
        ShowInstructable = true;
    }

    [RelayCommand]
    private async Task OpenInfoAsync()
    {
        await RunWithLoading(async () =>
        {
            await PerformNavigation(Routes.AppInfoPage, false);
        });
    }

    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        await RunWithLoading(async () =>
        {
            await PerformNavigation(Routes.SettingsPage, false);
        });
    }

    [RelayCommand]
    private async Task OnConfirmAsync(string page)
    {
        ShowInstructable = false;

        await RunWithLoading(async () =>
        {
            await PerformNavigation(page);
        });
    }

    public override void RefreshViewModel()
    {
        return;
    }

    public override async Task InitAsync()
    {
        if (!RequiresInitialzation)
            return;

        RequiresInitialzation = false;

        IsLoading = true;
        await Task.Yield();

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
        BeginText = TranslationService.GetString("Begin", "Begin");
        ConfirmText = TranslationService.GetString("GotIt", "Got it!");
        TapTo = TranslationService.GetString("ScrollTo", "Scroll to");
        Navigate = TranslationService.GetString("Navigate", "navigate");
        
        NextPageParameter = TranslationService.GetString("SurveyPage", "SurveyPage");

        IsLoading = false;
    }
}
