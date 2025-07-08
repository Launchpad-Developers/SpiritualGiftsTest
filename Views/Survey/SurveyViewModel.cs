using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpiritualGiftsSurvey.Helpers;
using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Views.Shared;
using System.Reflection;
using System.Runtime.Versioning;

namespace SpiritualGiftsSurvey.Views.Survey;

public class SurveyNavParameter
{
    public int TargetPage { get; set; }
    public string TargetPageTopic { get; set; } = string.Empty;
}

[SupportedOSPlatform("android")]
[SupportedOSPlatform("ios")]
public partial class SurveyViewModel : BaseViewModel
{

    public Dictionary<int, ContentView> ContentViews { get; set; } = new();

    public SurveyViewModel(
        IAggregatedServices aggregatedServices,
        IPreferences preferences) : base(aggregatedServices, preferences)
    {
        InitializeData();
    }

    public async void InitializeData()
    {
        FlowDirection = TranslationService.FlowDirection;

        Title = TranslationService.GetString("AppTitle", "Spiritual Gifts Survey");
        LoadingText = TranslationService.GetString("Loading", "Loading");

        UpdateQuestionLabel(_currentPage);
    }

    private int _currentPage = 1;
    private void UpdateQuestionLabel(int currentPage)
    {
        var question = TranslationService.GetString("Question", "question");
        var of = TranslationService.GetString("Of", "of");

        QuestionOf = $"{question} {currentPage} {of} {TranslationService.TotalQuestions}";
    }


    [ObservableProperty]
    private string nextPage = string.Empty;

    [ObservableProperty]
    private int pageNumber;

    [ObservableProperty]
    private bool showBackNav;

    [ObservableProperty]
    private bool showForwardNav;

    [RelayCommand]
    private async Task OnLeaveSurveyAsync()
    {
        var result = await ConfirmUserAsync(
            TranslationService.GetString("Quit", "quit"),
            TranslationService.GetString("AreYouSure", "Are you sure?"),
            TranslationService.GetString("Yes", "Yes"),
            TranslationService.GetString("No", "No"));

        if (result)
            await NavBack();
    }

    [RelayCommand]
    private void Navigate(SurveyNavParameter parameter)
    {
        UpdateQuestionLabel(++_currentPage);

        //TODO Consider making this what changes the question
        PageTopic = parameter.TargetPageTopic;
    }
}
