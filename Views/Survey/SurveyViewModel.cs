using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SpiritualGiftsSurvey.Messages;
using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Routing;
using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Views.Shared;
using System.Collections.ObjectModel;
using System.Runtime.Versioning;

namespace SpiritualGiftsSurvey.Views.Survey;


[SupportedOSPlatform("android")]
[SupportedOSPlatform("ios")]
public partial class SurveyViewModel : BaseViewModel
{

    private int _currentPage = 1;

    public SurveyViewModel(
        IAggregatedServices aggregatedServices,
        IPreferences preferences) : base(aggregatedServices, preferences)
    {
    }
    
    [ObservableProperty]
    private bool showConfirmButton;

    public ObservableCollection<QuestionViewModel> Questions { get; set; } = new();

    public override async void InitAsync()
    {
        IsLoading = true;
        FlowDirection = TranslationService.FlowDirection;

        PageTopic = TranslationService.GetString("AppTitle", "Spiritual Gifts Survey");
        LoadingText = TranslationService.GetString("Loading", "Loading");
        NavButtonText = TranslationService.GetString("Continue", "Continue");

        // Get all questions from the database
        var questions = DatabaseService.GetQuestions(TranslationService.CurrentLanguageCode);
        var totalQuestions = DatabaseService.GetQuestionsCount(TranslationService.CurrentLanguageCode);

        // Shuffle questions randomly
        var random = new Random();
        var shuffledQuestions = questions.OrderBy(_ => random.Next()).ToList();

        int index = 1;
        foreach (var q in shuffledQuestions)
        {
            var questionVm = new QuestionViewModel
            {
                QuestionText = q.QuestionText,
                QuestionId = q.QuestionGuid,
                NotAtAll = TranslationService.GetString("NotAtAll", "Not at all"),
                Little = TranslationService.GetString("Little", "Little"),
                Some = TranslationService.GetString("Some", "Some"),
                Much = TranslationService.GetString("Much", "Much"),
                QuestionOf = $"Question {index} of {totalQuestions}",
                ShowButtons = true
            };

            if (index == 1)
            {
                questionVm.QuestionMargin = new Thickness(30, 30, 30, 10);
            }

            if (index == totalQuestions)
            {
                questionVm.QuestionMargin = new Thickness(30, 10, 30, 100);
            }

            Questions.Add(questionVm);

            index++;
        }

        UpdateQuestionLabel(_currentPage);
        IsLoading = false;
    }

    [RelayCommand]
    private void ReachedEnd()
    {
        ShowConfirmButton = true;
    }

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
    private async Task FinishAsync()
    {
        QuestionViewModel? firstUnanswered = null;

        foreach (var q in Questions)
        {
            if (q.UserValue == Enums.UserValue.DidNotAnswer)
            {
                q.MarkQuestionUnanswered();

                if (firstUnanswered == null)
                    firstUnanswered = q;
            }
        }

        if (firstUnanswered != null)
        {
            // Optional: show message
            await NotifyUserAsync(
                TranslationService.GetString("Incomplete", "Incomplete"),
                TranslationService.GetString("PleaseAnswerAllQuestions", "Please answer all questions."),
                TranslationService.GetString("OK", "OK"));

            var index = Questions.IndexOf(firstUnanswered);

            WeakReferenceMessenger.Default.Send(new ScrollToQuestionMessage(index));
            return;
        }

        await NavigationService.NavigateAsync(Routes.ResultsPage);
    }

}
