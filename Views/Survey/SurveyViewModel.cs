using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SpiritualGiftsSurvey.Enums;
using SpiritualGiftsSurvey.Helpers;
using SpiritualGiftsSurvey.Messages;
using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Routing;
using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Utilities;
using SpiritualGiftsSurvey.Views.Controls;
using SpiritualGiftsSurvey.Views.Shared;
using System.Collections.ObjectModel;
using System.Runtime.Versioning;

namespace SpiritualGiftsSurvey.Views.Survey;

[SupportedOSPlatform("android")]
[SupportedOSPlatform("ios")]
public partial class SurveyViewModel(
    IAggregatedServices aggregatedServices,
    IPreferences preferences)
    : BaseViewModel(aggregatedServices, preferences)
{
    private int _currentPage = 1;

    [ObservableProperty] private bool showConfirmButton;
    [ObservableProperty] private string continueText = string.Empty;
    [ObservableProperty] private int pageNumber;
    [ObservableProperty] private bool showBackNav;
    [ObservableProperty] private bool showForwardNav;

    public ObservableCollection<QuestionViewModel> Questions { get; set; } = new();

    public override async Task InitAsync()
    {
        IsLoading = true;
        await Task.Yield();

        FlowDirection = TranslationService.FlowDirection;

        PageTopic = TranslationService.GetString("AppTitle", "Spiritual Gifts Survey");
        LoadingText = TranslationService.GetString("Loading", "Loading");
        ContinueText = TranslationService.GetString("Continue", "Continue");

        // Get all questions from the database
        var questions = await DatabaseService.GetQuestionsAsync(TranslationService.CurrentLanguageCode);
        var totalQuestions = await DatabaseService.GetQuestionsCountAsync(TranslationService.CurrentLanguageCode);

        // Shuffle questions randomly
        var random = new Random();
        var shuffledQuestions = questions.OrderBy(_ => random.Next()).ToList();

#if DEBUG
        var debugUserValues = DebugHelper.ApplyDebugQuestionFilters(ref shuffledQuestions, random);
#endif

        totalQuestions = shuffledQuestions.Count;

        int index = 1;
        foreach (var q in shuffledQuestions)
        {
            var questionVm = new QuestionViewModel
            {
                QuestionText = q.QuestionText,
                QuestionId = q.QuestionGuid,
                Gift = q.Gift,
                NotAtAll = TranslationService.GetString("NotAtAll", "Not at all"),
                Little = TranslationService.GetString("Little", "Little"),
                Some = TranslationService.GetString("Some", "Some"),
                Much = TranslationService.GetString("Much", "Much"),
                QuestionOf = $"Question {index} of {totalQuestions}",
                ShowButtons = true,
                GiftDescriptionGuid = q.GiftDescriptionGuid,
            };

#if DEBUG
            // Apply debug answer if set
            if (debugUserValues.TryGetValue(q.QuestionGuid, out var debugValue))
            {
                questionVm.UserValue = debugValue;

                if (debugValue == UserValue.DidNotAnswer)
                    questionVm.MarkQuestionUnanswered();
            }
#endif

            if (index == 1)
                questionVm.QuestionMargin = new Thickness(30, 30, 30, 10);

            if (index == totalQuestions)
                questionVm.QuestionMargin = new Thickness(30, 10, 30, 100);

            Questions.Add(questionVm);

            //Yield every 5 items to keep UI responsive
            //Prevents janky behavior on slower devices
            if (index % 5 == 0)
                await Task.Yield();

            index++;
        }

#if DEBUG
        WeakReferenceMessenger.Default.Send(new ScrollToQuestionMessage(totalQuestions - 1));
#endif


        UpdateQuestionLabel(_currentPage);

        for (int i = 0; i < Questions.Count; i++)
        {
            Questions[i].IsFirst = (i == 0);
            Questions[i].IsLast = (i == Questions.Count - 1);
        }

        IsLoading = false;
    }

    public override void RefreshViewModel()
    {
        Questions.Clear();
        ShowConfirmButton = false;
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

    [RelayCommand]
    private async Task OnLeaveSurveyAsync()
    {
        var result = await ConfirmUserAsync(
            TranslationService.GetString("Quit", "quit"),
            TranslationService.GetString("AreYouSure", "Are you sure?"),
            TranslationService.GetString("Yes", "Yes"),
            TranslationService.GetString("No", "No"));

        if (result)
            await NavBack(Routes.WelcomePage);
    }

    [RelayCommand]
    private async Task FinishAsync()
    {
        IsLoading = true;
        await Task.Yield();

        QuestionViewModel? firstUnanswered = null;

        foreach (var q in Questions)
        {
            if (q.UserValue != UserValue.DidNotAnswer) continue;
            q.MarkQuestionUnanswered();
            firstUnanswered ??= q;
        }

        if (firstUnanswered != null)
        {
            IsLoading = false;

            await NotifyUserAsync(
                TranslationService.GetString("Incomplete", "Incomplete"),
                TranslationService.GetString("PleaseAnswerAllQuestions", "Please answer all questions."),
                TranslationService.GetString("OK", "OK"));

            var index = Questions.IndexOf(firstUnanswered);
            WeakReferenceMessenger.Default.Send(new ScrollToQuestionMessage(index));
            return;
        }

        // Tally how many questions exist for each gift
        var giftCounts = Questions
            .GroupBy(q => q.Gift)
            .ToDictionary(
                g => g.Key,
                g => g.Count()
            );

        // Generate UserGiftScore list including Score and MaxScore
        var giftScores = Questions
            .GroupBy(q => q.Gift)
            .Select(group =>
            {
                var gift = group.Key;
                var score = group.Sum(q => (int)q.UserValue);
                var maxScore = giftCounts[gift] * 3; // max per question = 3
                var giftGuid = group.Select(q => q.GiftDescriptionGuid).FirstOrDefault();

                return new UserGiftScore
                {
                    GiftName = gift.ToString(),
                    Score = score,
                    MaxScore = maxScore,
                    Gift = gift,
                    GiftDescriptionGuid = giftGuid
                };
            })
            .OrderByDescending(gs => gs.Score)
            .ToList();

        var result = new SurveyResult
        {
            DateTaken = DateTime.UtcNow,
            Scores = giftScores
        };

        await result.RankGiftsAsync();

        await DatabaseService.SaveUserGiftResultAsync(result);

        await NavigationService.NavigateAsync(Routes.ResultsPage, new Dictionary<string, object>
        {
            ["UserGiftResult"] = result
        });

        IsLoading = false;
    }
}
