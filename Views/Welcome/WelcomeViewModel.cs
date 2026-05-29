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
        Console.WriteLine($"[WelcomeViewModel.RefreshViewModel] ========================================");
        Console.WriteLine($"[WelcomeViewModel.RefreshViewModel] Called");
        Console.WriteLine($"[WelcomeViewModel.RefreshViewModel] ViewModel HashCode: {this.GetHashCode()}");
        Console.WriteLine($"[WelcomeViewModel.RefreshViewModel] Current BeginText: '{BeginText}'");
        Console.WriteLine($"[WelcomeViewModel.RefreshViewModel] Questions.Count: {Questions.Count}");
        Console.WriteLine($"[WelcomeViewModel.RefreshViewModel] RequiresInitialzation: {RequiresInitialzation}");
        Console.WriteLine($"[WelcomeViewModel.RefreshViewModel] IsLoading: {IsLoading}");
        
        // Don't do anything if we're currently loading - let InitAsync finish
        if (IsLoading)
        {
            Console.WriteLine($"[WelcomeViewModel.RefreshViewModel] IsLoading = true, skipping refresh to let InitAsync complete");
            Console.WriteLine($"[WelcomeViewModel.RefreshViewModel] ========================================");
            return;
        }
        
        // Force UI update by re-setting all properties
        Console.WriteLine($"[WelcomeViewModel.RefreshViewModel] Forcing property notifications...");
        OnPropertyChanged(nameof(BeginText));
        OnPropertyChanged(nameof(Questions));
        OnPropertyChanged(nameof(PageTopic));
        OnPropertyChanged(nameof(IsLoading));
        Console.WriteLine($"[WelcomeViewModel.RefreshViewModel] Property notifications sent");
        
        // If we previously failed initialization (BeginText == "Retry"), try again
        if (BeginText == "Retry" || string.IsNullOrEmpty(BeginText))
        {
            Console.WriteLine($"[WelcomeViewModel.RefreshViewModel] Detected retry state - forcing re-initialization");
            RequiresInitialzation = true;
            
            // Re-run InitAsync on the main thread
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await InitAsync();
            });
        }
        else
        {
            Console.WriteLine($"[WelcomeViewModel.RefreshViewModel] Already initialized successfully - no action needed");
        }
        Console.WriteLine($"[WelcomeViewModel.RefreshViewModel] ========================================");
    }

    public override async Task InitAsync()
    {
        Console.WriteLine($"[WelcomeViewModel] ========================================");
        Console.WriteLine($"[WelcomeViewModel] Starting InitAsync");
        Console.WriteLine($"[WelcomeViewModel] RequiresInitialization: {RequiresInitialzation}");
        
        if (!RequiresInitialzation)
        {
            Console.WriteLine($"[WelcomeViewModel] Skipping initialization - already initialized");
            return;
        }

        RequiresInitialzation = false;

        try
        {
            Console.WriteLine($"[WelcomeViewModel] Setting IsLoading = true");
            IsLoading = true;
            await Task.Yield();

            Console.WriteLine($"[WelcomeViewModel] Calling TranslationService.InitializeLanguage()...");
            var languageInitialized = await TranslationService.InitializeLanguage();
            Console.WriteLine($"[WelcomeViewModel] InitializeLanguage result: {languageInitialized}");

            if (!languageInitialized)
            {
                Console.WriteLine($"[WelcomeViewModel] ⚠️ Language initialization failed - attempting database refresh");
                // Database might be empty or corrupted, show error message
                LoadingText = "Loading database...";
                
                // Attempt to refresh database from Firebase
                var refreshSuccess = await DatabaseService.RefreshDatabaseAsync();
                Console.WriteLine($"[WelcomeViewModel] Database refresh result: {refreshSuccess}");
                
                if (refreshSuccess)
                {
                    // Retry initialization after refresh
                    Console.WriteLine($"[WelcomeViewModel] Retrying InitializeLanguage after refresh...");
                    languageInitialized = await TranslationService.InitializeLanguage();
                    Console.WriteLine($"[WelcomeViewModel] Second InitializeLanguage result: {languageInitialized}");
                }
                
                if (!languageInitialized)
                {
                    Console.WriteLine($"[WelcomeViewModel] ❌ Language initialization failed after retry - showing fallback UI");
                    // Still failed - show default instructions with fallback message
                    Questions.Add(new QuestionViewModel 
                    { 
                        QuestionText = "Welcome to the Spiritual Gifts Survey.\n\nUnable to load instructions. Please check your internet connection and restart the app." 
                    });
                    
                    BeginText = "Retry";
                    PageTopic = "Spiritual Gifts Survey";
                    IsLoading = false;
                    Console.WriteLine($"[WelcomeViewModel] ========================================");
                    return;
                }
            }

            Console.WriteLine($"[WelcomeViewModel] Questions.Count before RemoveAt: {Questions.Count}");
            if (Questions.Count > 0)
            {
                Questions.RemoveAt(0);
                Console.WriteLine($"[WelcomeViewModel] Removed first question");
            };

            Console.WriteLine($"[WelcomeViewModel] Fetching Instructions string...");
            string instructionsKey = TranslationService.GetString("Instructions", "DEFAULT_INSTRUCTIONS");
            Console.WriteLine($"[WelcomeViewModel] Instructions returned: '{instructionsKey?.Substring(0, Math.Min(200, instructionsKey?.Length ?? 0))}...'");
            Console.WriteLine($"[WelcomeViewModel] Instructions full length: {instructionsKey?.Length ?? 0} chars");
            
            // Use string.Format to replace {0} placeholders with actual line breaks
            string instructions = string.Format(instructionsKey, Environment.NewLine);
            Console.WriteLine($"[WelcomeViewModel] Instructions formatted (first 200 chars): '{instructions?.Substring(0, Math.Min(200, instructions?.Length ?? 0))}...'");
            Console.WriteLine($"[WelcomeViewModel] Instructions final length: {instructions?.Length ?? 0} chars");
            
            Questions.Add(new QuestionViewModel { QuestionText = instructions });
            Console.WriteLine($"[WelcomeViewModel] Added instructions to Questions - Count now: {Questions.Count}");

            FlowDirection = TranslationService.FlowDirection;
            LoadingText = TranslationService.GetString("Loading", "Loading");
            PageTopic = TranslationService.GetString("AppTitle", "Spiritual Gift Survey");
            
            Console.WriteLine($"[WelcomeViewModel] About to set BeginText...");
            BeginText = TranslationService.GetString("Begin", "Begin");
            Console.WriteLine($"[WelcomeViewModel] BeginText property set to: '{BeginText}'");
            
            ConfirmText = TranslationService.GetString("GotIt", "Got it!");
            TapTo = TranslationService.GetString("ScrollTo", "Scroll to");
            Navigate = TranslationService.GetString("Navigate", "navigate");
            
            NextPageParameter = TranslationService.GetString("SurveyPage", "SurveyPage");
            
            Console.WriteLine($"[WelcomeViewModel] ✅ InitAsync completed successfully");
            Console.WriteLine($"[WelcomeViewModel]   PageTopic: {PageTopic}");
            Console.WriteLine($"[WelcomeViewModel]   BeginText: {BeginText}");
            Console.WriteLine($"[WelcomeViewModel]   Questions count: {Questions.Count}");
            Console.WriteLine($"[WelcomeViewModel]   About to exit try block and enter finally...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WelcomeViewModel] ❌ EXCEPTION in InitAsync");
            Console.WriteLine($"[WelcomeViewModel] Exception: {ex.Message}");
            Console.WriteLine($"[WelcomeViewModel] Exception Type: {ex.GetType().Name}");
            Console.WriteLine($"[WelcomeViewModel] Stack Trace: {ex.StackTrace}");
            
            // Log error for diagnostics
            Analytics.TrackEvent("WelcomeInitFailure", 
                new Dictionary<string, string> 
                { 
                    { "Error", ex.Message },
                    { "StackTrace", ex.StackTrace ?? "none" }
                });

            // Show fallback UI
            Questions.Add(new QuestionViewModel 
            { 
                QuestionText = "Welcome to the Spiritual Gifts Survey.\n\nAn error occurred while loading. Please restart the app." 
            });
            
            BeginText = "Start";
            PageTopic = "Spiritual Gifts Survey";
        }
        finally
        {
            Console.WriteLine($"[WelcomeViewModel] 🔹 FINALLY block executing");
            Console.WriteLine($"[WelcomeViewModel] Setting IsLoading = false (was: {IsLoading})");
            IsLoading = false;
            Console.WriteLine($"[WelcomeViewModel] IsLoading is now: {IsLoading}");
            Console.WriteLine($"[WelcomeViewModel] ========================================");
        }
    }
}
