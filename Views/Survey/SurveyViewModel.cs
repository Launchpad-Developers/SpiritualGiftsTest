using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpiritualGiftsTest.Helpers;
using SpiritualGiftsTest.Services;
using SpiritualGiftsTest.Views.Shared;
using System.Runtime.Versioning;

namespace SpiritualGiftsTest.Views.Survey;

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
        CurrentTranslation = TranslationService.Language;

        if (CurrentTranslation == null)
        {
            await NotifyUserAsync(
                "Error",
                "Translation not found",
                "Ok");

            //TODO Log error

            return;
        }

        FlowDirection = CurrentTranslation.FlowDirection.Equals("RTL") ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        SpiritualGiftsTest = CurrentTranslation.AppStrings.Get("AppTitle", "Spiritual Gifts Survey");
        LoadingText = CurrentTranslation.AppStrings.Get("Loading", "Loading");

        UpdateQuestionLabel(_currentPage);
    }

    private int _currentPage = 1;
    private void UpdateQuestionLabel(int currentPage)
    {
        var question = CurrentTranslation.AppStrings.Get("Question", "question");
        var of = CurrentTranslation.AppStrings.Get("Of", "of");
        var total = CurrentTranslation.Questions.Count;

        QuestionOf = $"{question} {currentPage} {of} {total}";
    }


    [ObservableProperty]
    private string nextPage = string.Empty;

    [ObservableProperty]
    private int pageNumber;

    [ObservableProperty]
    private bool showBackNav;

    [ObservableProperty]
    private bool showForwardNav;

    [ObservableProperty]
    private string spiritualGiftsTest = string.Empty;

    [RelayCommand]
    private async Task OnLeaveSurveyAsync()
    {
        var result = await ConfirmUserAsync(
            CurrentTranslation.AppStrings.Get("Quit", "quit"),
            CurrentTranslation.AppStrings.Get("AreYouSure", "Are you sure?"),
            CurrentTranslation.AppStrings.Get("Yes", "Yes"),
            CurrentTranslation.AppStrings.Get("No", "No"));

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
