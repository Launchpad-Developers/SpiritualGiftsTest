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
    [ObservableProperty] private string continueText = string.Empty;

    // Store ALL questions (full shuffled set)
    public ObservableCollection<QuestionViewModel> AllQuestions { get; set; } = new();

    public override async Task InitAsync()
    {
        Console.WriteLine($"[SurveyViewModel] ========================================");
        Console.WriteLine($"[SurveyViewModel] Starting InitAsync");
        
        try
        {
            Console.WriteLine($"[SurveyViewModel] Setting IsLoading = true");
            IsLoading = true;
            await Task.Yield();

            FlowDirection = TranslationService.FlowDirection;
            Console.WriteLine($"[SurveyViewModel] FlowDirection: {FlowDirection}");

            PageTopic = TranslationService.GetString("AppTitle", "Spiritual Gifts Survey");
            LoadingText = TranslationService.GetString("Loading", "Loading");
            ContinueText = TranslationService.GetString("Continue", "Continue");
            
            Console.WriteLine($"[SurveyViewModel] Current Language: {TranslationService.CurrentLanguageCode}");

            // Check for existing progress
            Console.WriteLine($"[SurveyViewModel] Checking for existing survey progress...");
            var existingProgress = await SurveyProgressService.GetActiveProgressAsync();

            if (existingProgress != null)
            {
                Console.WriteLine($"[SurveyViewModel] ✅ Found existing progress - restoring survey state");
                await RestoreProgressAsync(existingProgress);
                return;
            }

            Console.WriteLine($"[SurveyViewModel] No existing progress - starting new survey");

            // Get all questions from the database
            Console.WriteLine($"[SurveyViewModel] Fetching questions from database...");
            var questions = await DatabaseService.GetQuestionsAsync(TranslationService.CurrentLanguageCode);
            Console.WriteLine($"[SurveyViewModel] Questions fetched - Count: {questions?.Count ?? 0}");
            
            var totalQuestions = await DatabaseService.GetQuestionsCountAsync(TranslationService.CurrentLanguageCode);
            Console.WriteLine($"[SurveyViewModel] Total questions count: {totalQuestions}");

            // If no questions available, notify user and navigate back
            if (questions == null || questions.Count == 0)
            {
                Console.WriteLine($"[SurveyViewModel] ❌ No questions available - navigating back to Welcome");
                await NotifyUserAsync(
                    TranslationService.GetString("Error", "Error"),
                    TranslationService.GetString("NoQuestionsAvailable", "Unable to load survey questions. Please check your connection and try again."),
                    TranslationService.GetString("OK", "OK"));
                
                await NavBack(Routes.WelcomePage);
                Console.WriteLine($"[SurveyViewModel] ========================================");
                return;
            }

            // Shuffle questions randomly
            Console.WriteLine($"[SurveyViewModel] Shuffling questions...");
            var random = new Random();
            var shuffledQuestions = questions.OrderBy(_ => random.Next()).ToList();

#if DEBUG
            Console.WriteLine($"[SurveyViewModel] DEBUG mode - applying debug filters...");
            var debugUserValues = DebugHelper.ApplyDebugQuestionFilters(ref shuffledQuestions, random);
            Console.WriteLine($"[SurveyViewModel] After debug filters - Count: {shuffledQuestions.Count}");
#endif

            totalQuestions = shuffledQuestions.Count;
            Console.WriteLine($"[SurveyViewModel] Final questions count after shuffle/filter: {totalQuestions}");

            Console.WriteLine($"[SurveyViewModel] Creating QuestionViewModel objects...");
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

                // Hook up answer change callback for auto-save
                questionVm.OnAnswerChanged = async (qvm) => await OnQuestionAnsweredAsync(qvm);

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

                AllQuestions.Add(questionVm);

                //Yield every 5 items to keep UI responsive
                //Prevents janky behavior on slower devices
                if (index % 5 == 0)
                    await Task.Yield();

                index++;
            }

            // Save initial progress with shuffled question order
            Console.WriteLine($"[SurveyViewModel] Saving initial survey progress...");
            await SaveInitialProgressAsync();

#if DEBUG
            WeakReferenceMessenger.Default.Send(new ScrollToQuestionMessage(totalQuestions - 1));
#endif

            Console.WriteLine($"[SurveyViewModel] Updating question labels and properties...");
            UpdateQuestionLabel();

            for (int i = 0; i < AllQuestions.Count; i++)
            {
                AllQuestions[i].IsFirst = (i == 0);
                AllQuestions[i].IsLast = (i == AllQuestions.Count - 1);
            }
            
            Console.WriteLine($"[SurveyViewModel] ✅ InitAsync completed successfully");
            Console.WriteLine($"[SurveyViewModel]   Total Questions: {AllQuestions.Count}");
            Console.WriteLine($"[SurveyViewModel]   PageTopic: {PageTopic}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SurveyViewModel] ❌ EXCEPTION in InitAsync");
            Console.WriteLine($"[SurveyViewModel] Exception: {ex.Message}");
            Console.WriteLine($"[SurveyViewModel] Exception Type: {ex.GetType().Name}");
            Console.WriteLine($"[SurveyViewModel] Stack Trace: {ex.StackTrace}");
            
            // Log error for diagnostics
            Analytics.TrackEvent("SurveyLoadFailure", 
                new Dictionary<string, string> 
                { 
                    { "Error", ex.Message },
                    { "StackTrace", ex.StackTrace ?? "none" }
                });

            // Show user-friendly error message
            await NotifyUserAsync(
                TranslationService.GetString("Error", "Error"),
                TranslationService.GetString("SurveyLoadError", "Unable to load the survey. Please try again."),
                TranslationService.GetString("OK", "OK"));
            
            // Navigate back to welcome page
            await NavBack(Routes.WelcomePage);
        }
        finally
        {
            // Always ensure loading spinner is turned off
            Console.WriteLine($"[SurveyViewModel] Setting IsLoading = false");
            IsLoading = false;
            Console.WriteLine($"[SurveyViewModel] ========================================");
        }
    }

    public override void RefreshViewModel()
    {
        AllQuestions.Clear();
    }

    [RelayCommand]
    private void ReachedEnd()
    {
        // No longer needed - button is always visible
    }

    private async Task SaveInitialProgressAsync()
    {
        var questionOrder = AllQuestions.Select(q => q.QuestionId).ToList();
        var answers = new Dictionary<Guid, UserValue>();

        var progress = new SurveyProgress
        {
            SessionGuid = Guid.NewGuid(),
            StartedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            LanguageCode = TranslationService.CurrentLanguageCode,
            CurrentPage = 1,  // No longer used but kept for compatibility
            QuestionOrderJson = System.Text.Json.JsonSerializer.Serialize(questionOrder),
            AnswersJson = System.Text.Json.JsonSerializer.Serialize(answers)
        };

        await SurveyProgressService.SaveProgressAsync(progress);
        Console.WriteLine($"[SurveyViewModel] Initial progress saved - Session: {progress.SessionGuid}");
    }

    private async Task RestoreProgressAsync(SurveyProgress progress)
    {
        Console.WriteLine($"[SurveyViewModel] Restoring progress from session: {progress.SessionGuid}");
        Console.WriteLine($"[SurveyViewModel] Progress started: {progress.StartedAt}, last updated: {progress.LastUpdatedAt}");

        // Verify language match
        if (progress.LanguageCode != TranslationService.CurrentLanguageCode)
        {
            Console.WriteLine($"[SurveyViewModel] ⚠️ Language mismatch - clearing progress and starting fresh");
            await SurveyProgressService.ClearProgressAsync();
            await InitAsync();  // Recursive call to start fresh
            return;
        }

        try
        {
            // Deserialize saved question order
            var questionOrder = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(progress.QuestionOrderJson);
            if (questionOrder == null || questionOrder.Count == 0)
            {
                Console.WriteLine($"[SurveyViewModel] ⚠️ Invalid question order - starting fresh");
                await SurveyProgressService.ClearProgressAsync();
                await InitAsync();
                return;
            }

            // Fetch all questions
            var allQuestions = await DatabaseService.GetQuestionsAsync(TranslationService.CurrentLanguageCode);
            if (allQuestions == null || allQuestions.Count != questionOrder.Count)
            {
                Console.WriteLine($"[SurveyViewModel] ⚠️ Question count mismatch - starting fresh");
                await SurveyProgressService.ClearProgressAsync();
                await InitAsync();
                return;
            }

            // Create question lookup
            var questionLookup = allQuestions.ToDictionary(q => q.QuestionGuid);

            // Deserialize saved answers
            var answers = System.Text.Json.JsonSerializer.Deserialize<Dictionary<Guid, UserValue>>(progress.AnswersJson);
            answers ??= new Dictionary<Guid, UserValue>();

            Console.WriteLine($"[SurveyViewModel] Restored {questionOrder.Count} questions and {answers.Count} answers");

            // Rebuild AllQuestions in the saved order
            int index = 1;
            foreach (var questionGuid in questionOrder)
            {
                if (!questionLookup.TryGetValue(questionGuid, out var q))
                {
                    Console.WriteLine($"[SurveyViewModel] ⚠️ Question GUID not found - starting fresh");
                    await SurveyProgressService.ClearProgressAsync();
                    await InitAsync();
                    return;
                }

                var questionVm = new QuestionViewModel
                {
                    QuestionText = q.QuestionText,
                    QuestionId = q.QuestionGuid,
                    Gift = q.Gift,
                    NotAtAll = TranslationService.GetString("NotAtAll", "Not at all"),
                    Little = TranslationService.GetString("Little", "Little"),
                    Some = TranslationService.GetString("Some", "Some"),
                    Much = TranslationService.GetString("Much", "Much"),
                    QuestionOf = $"Question {index} of {questionOrder.Count}",
                    ShowButtons = true,
                    GiftDescriptionGuid = q.GiftDescriptionGuid,
                };

                // Restore saved answer
                if (answers.TryGetValue(questionGuid, out var savedValue) && savedValue != UserValue.DidNotAnswer)
                {
                    questionVm.UserValue = savedValue;
                    questionVm.Answered = true;
                    
                    // Set the correct button color for the saved answer
                    switch (savedValue)
                    {
                        case UserValue.NotAtAll:
                            questionVm.NotAtAllButtonColor = Application.Current?.Resources.TryGetValue("EmeraldGreen", out var notAtAllColor) == true && notAtAllColor is Color c1 ? c1 : Colors.Green;
                            break;
                        case UserValue.Little:
                            questionVm.LittleButtonColor = Application.Current?.Resources.TryGetValue("EmeraldGreen", out var littleColor) == true && littleColor is Color c2 ? c2 : Colors.Green;
                            break;
                        case UserValue.Some:
                            questionVm.SomeButtonColor = Application.Current?.Resources.TryGetValue("EmeraldGreen", out var someColor) == true && someColor is Color c3 ? c3 : Colors.Green;
                            break;
                        case UserValue.Much:
                            questionVm.MuchButtonColor = Application.Current?.Resources.TryGetValue("EmeraldGreen", out var muchColor) == true && muchColor is Color c4 ? c4 : Colors.Green;
                            break;
                    }
                    
                    // Set border to green for answered questions
                    questionVm.BorderColor = Application.Current?.Resources.TryGetValue("EmeraldGreen", out var borderColor) == true && borderColor is Color c5 ? c5 : Colors.Green;
                }

                // Hook up answer change callback for auto-save
                questionVm.OnAnswerChanged = async (qvm) => await OnQuestionAnsweredAsync(qvm);

                if (index == 1)
                    questionVm.QuestionMargin = new Thickness(30, 30, 30, 10);

                if (index == questionOrder.Count)
                    questionVm.QuestionMargin = new Thickness(30, 10, 30, 100);

                AllQuestions.Add(questionVm);

                if (index % 5 == 0)
                    await Task.Yield();

                index++;
            }

            UpdateQuestionLabel();

            for (int i = 0; i < AllQuestions.Count; i++)
            {
                AllQuestions[i].IsFirst = (i == 0);
                AllQuestions[i].IsLast = (i == AllQuestions.Count - 1);
            }

            Console.WriteLine($"[SurveyViewModel] ✅ Progress restored successfully");
            Console.WriteLine($"[SurveyViewModel]   Answered Questions: {answers.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SurveyViewModel] ❌ Error restoring progress: {ex.Message}");
            Console.WriteLine($"[SurveyViewModel] Starting fresh survey");
            await SurveyProgressService.ClearProgressAsync();
            await InitAsync();
        }
    }

    private async Task OnQuestionAnsweredAsync(QuestionViewModel questionVm)
    {
        Console.WriteLine($"[SurveyViewModel] Question answered: {questionVm.QuestionId} = {questionVm.UserValue}");
        
        try
        {
            await SurveyProgressService.UpdateAnswerAsync(questionVm.QuestionId, questionVm.UserValue);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SurveyViewModel] ⚠️ Failed to save answer: {ex.Message}");
            // Don't block UI on save failure - progress will be saved on next page change
        }
    }

    private void UpdateQuestionLabel()
    {
        var question = TranslationService.GetString("Question", "question");
        var of = TranslationService.GetString("Of", "of");

        QuestionOf = $"{question} 1 {of} {TranslationService.TotalQuestions}";
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

        // Check ALL questions for unanswered (not just current page)
        foreach (var q in AllQuestions)
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

            // Scroll to the first unanswered question
            var index = AllQuestions.IndexOf(firstUnanswered);
            WeakReferenceMessenger.Default.Send(new ScrollToQuestionMessage(index));
            return;
        }

        // Tally how many questions exist for each gift
        var giftCounts = AllQuestions
            .GroupBy(q => q.Gift)
            .ToDictionary(
                g => g.Key,
                g => g.Count()
            );

        // Generate UserGiftScore list including Score and MaxScore
        var giftScores = AllQuestions
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

        // Clear saved progress after successful completion
        Console.WriteLine($"[SurveyViewModel] Survey completed - clearing saved progress");
        await SurveyProgressService.ClearProgressAsync();

        await NavigationService.NavigateAsync(Routes.ResultsPage, new Dictionary<string, object>
        {
            ["UserGiftResult"] = result
        });

        IsLoading = false;
    }
}
