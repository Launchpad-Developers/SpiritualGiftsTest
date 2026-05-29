using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpiritualGiftsSurvey.Helpers;
using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Utilities;
using SpiritualGiftsSurvey.Views.Shared;
using System.Collections.ObjectModel;
using System.Runtime.Versioning;

namespace SpiritualGiftsSurvey.Views.Settings;

[SupportedOSPlatform("android")]
[SupportedOSPlatform("ios")]
public partial class SettingsViewModel(
    IAggregatedServices aggregatedServices,
    IPreferences preferences)
    : BaseViewModel(aggregatedServices, preferences)
{
    [ObservableProperty] private string header = string.Empty;
    [ObservableProperty] private List<LanguageOption> languageOptions = new();
    [ObservableProperty] private LanguageOption? selectedLanguage;
    [ObservableProperty] private string languageTitle = string.Empty;
    [ObservableProperty] private string currentLanguageTitle = string.Empty;
    [ObservableProperty] private string newReportingEmail = string.Empty;
    [ObservableProperty] private string restoreDatabase = string.Empty;
    [ObservableProperty] private string clearData = string.Empty;
    [ObservableProperty] private string reportingEmailsTitle = string.Empty;
    [ObservableProperty] private string addReportingEmailPlaceholder = string.Empty;
    [ObservableProperty] private bool showLanguagePicker;
    [ObservableProperty] private bool showCollectionView;
    [ObservableProperty] private int totalTopics;
    [ObservableProperty] private int topicLimit;
    [ObservableProperty] private int totalQuestions;
    [ObservableProperty] private int totalUnansweredQuestions;
    [ObservableProperty] private bool debugOptionsEnabled;
    [ObservableProperty] private bool showDebugOptions;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowUnansweredQuestionControls))]
    private bool allowUnansweredQuestions;

    [ObservableProperty]
    private Color addReportingEmailTextColor = Colors.Black;

    private string _yes = string.Empty;
    private string _no = string.Empty;
    private string _ok = string.Empty;
    private string _restoreDatabaseInfo = string.Empty;
    private string _clearDataWarning = string.Empty;

    private string _restoreCompleteTitle = string.Empty;
    private string _restoreCompleteMessage = string.Empty;
    private string _dataClearedTitle = string.Empty;
    private string _dataClearedMessage = string.Empty;
    private string _restoreFailedTitle = string.Empty;
    private string _restoreFailedMessage = string.Empty;

    private string _invalidEmailTitle = string.Empty;
    private string _invalidEmailMessage = string.Empty;

    private string _removeEmailTitle = string.Empty;
    private string _removeEmailMessage = string.Empty;

    private Color _cachedBlackColor = Colors.Black;
    private Color _cachedDangerRedColor = Colors.Red;

    public ObservableCollection<string> ReportingEmails { get; private set; } = new();
    public bool ShowUnansweredQuestionControls => AllowUnansweredQuestions;

    public override async Task InitAsync()
    {
        if (!RequiresInitialzation)
            return;

        RequiresInitialzation = false;

        IsLoading = true;
        await Task.Yield();

        // Cache color resources once
        _cachedBlackColor = (Application.Current?.Resources.TryGetValue("Black", out var blackValue) == true && blackValue is Color black)
            ? black
            : Colors.Black;
        _cachedDangerRedColor = (Application.Current?.Resources.TryGetValue("DangerRed", out var redValue) == true && redValue is Color red)
            ? red
            : Colors.Red;
        
        AddReportingEmailTextColor = _cachedBlackColor;

        ReportingEmails.Clear();
        foreach (var email in EmailService.GetStoredEmails())
        {
            ReportingEmails.Add(email);
        }

        FlowDirection = TranslationService.FlowDirection;

        PageTopic = TranslationService.GetString("Settings", "Settings");
        LanguageTitle = TranslationService.CurrentLanguageDisplayName;
        LanguageOptions = TranslationService.GetLanguageOptions();
        CurrentLanguageTitle = TranslationService.GetString("Language", "Language");
        ReportingEmailsTitle = TranslationService.GetString("ReportingEmailsTitle", "Reporting Emails");
        AddReportingEmailPlaceholder = TranslationService.GetString("AddReportingEmailPlaceholder", "Add reporting email");

        var currentCode = TranslationService.CurrentLanguageCode;
        // FIX: Add fallback if no matching language found
        SelectedLanguage = LanguageOptions.FirstOrDefault(lo => lo.CodeOption == currentCode)
                           ?? LanguageOptions.FirstOrDefault();

        LoadingText = TranslationService.GetString("Loading", "Loading");

        _yes = TranslationService.GetString("Yes", "Yes");
        _no = TranslationService.GetString("No", "No");
        _ok = TranslationService.GetString("OK", "OK");

        RestoreDatabase = TranslationService.GetString("RestoreDatabase", "Restore Database");
        _restoreDatabaseInfo = TranslationService.GetString("RestoreDatabaseInfo", "This action will restore the local database to it's most current form. Proceed?");
        
        // CRITICAL FIX: Use string literal keys instead of variable names
        _restoreCompleteTitle = TranslationService.GetString("RestoreCompleteTitle", "Restore Complete");
        _restoreCompleteMessage = TranslationService.GetString("RestoreCompleteMessage", "Your database has been restored. Please restart the app.");
        _restoreFailedTitle = TranslationService.GetString("RestoreFailedTitle", "Restore Failed");
        _restoreFailedMessage = TranslationService.GetString("RestoreFailedMessage", "There was an error restoring the database.");

        ClearData = TranslationService.GetString("ClearData", "Clear Data");
        _clearDataWarning = TranslationService.GetString("ClearDataWarning", "WARNING: This will clear all stored results from this device. Are you sure?");
        _dataClearedTitle = TranslationService.GetString("DataClearedTitle", "Data Cleared");
        _dataClearedMessage = TranslationService.GetString("DataClearedMessage", "Your data has been erased.");

        _invalidEmailTitle = TranslationService.GetString("InvalidEmailTitle", "Invalid Email");
        _invalidEmailMessage = TranslationService.GetString("InvalidEmailMessage", "Please enter a valid email address.");

        _removeEmailTitle = TranslationService.GetString("RemoveEmailTitle", "Remove Email");
        _removeEmailMessage = TranslationService.GetString("RemoveEmailMessage", "Remove {0} from the reporting list?");

        ShowCollectionView = ReportingEmails.Any();

#if DEBUG
        ShowDebugOptions = true;
        DebugOptionsEnabled = Preferences.Get(AppConstants.DebugOptionsEnabledKey, false);
        AllowUnansweredQuestions = Preferences.Get(AppConstants.DebugAllowUnansweredQuestionsKey, false);
        // FIX: Add bounds validation for debug preferences
        TotalTopics = Math.Clamp(Preferences.Get(AppConstants.DebugTotalTopicsKey, 0), 0, 50);
        TotalQuestions = Math.Clamp(Preferences.Get(AppConstants.DebugTotalQuestionsKey, 0), 0, 500);
        TopicLimit = Math.Clamp(Preferences.Get(AppConstants.DebugQuestionsPerTopicKey, 0), 0, 50);
        TotalUnansweredQuestions = Math.Clamp(Preferences.Get(AppConstants.DebugTotalUnansweredQuestionsKey, 0), 0, 100);
#endif

        IsLoading = false;
    }

    public override void RefreshViewModel()
    {
        return;
    }

    private bool _emailIsError;
    public void ResetAddReportingEmailEntry()
    {
        if (!_emailIsError) return;

        _emailIsError = false;

        // Use cached color
        AddReportingEmailTextColor = _cachedBlackColor;
    }

    [RelayCommand]
    private async Task AddNewReportingEmailAsync()
    {
        if (string.IsNullOrWhiteSpace(NewReportingEmail))
            return;

        var trimmedEmail = NewReportingEmail.Trim();

        if (!PageHelper.IsValidEmail(trimmedEmail))
        {
            _emailIsError = true;

            // Use cached color
            AddReportingEmailTextColor = _cachedDangerRedColor;

            await NotifyUserAsync(_invalidEmailTitle, _invalidEmailMessage, _ok);
            return;
        }

        // FIX: Add error handling around EmailService calls
        try
        {
            if (EmailService.SaveEmail(trimmedEmail))
            {
                ReportingEmails.Add(trimmedEmail);
            }

            NewReportingEmail = string.Empty;
            ShowCollectionView = ReportingEmails.Any();
        }
        catch (Exception ex)
        {
            Analytics.TrackEvent("EmailSaveFailure",
                new Dictionary<string, string>
                {
                    { "Error", ex.Message }
                });

            await NotifyUserAsync(
                TranslationService.GetString("ErrorTitle", "Error"),
                TranslationService.GetString("EmailSaveError", "Failed to save email. Please try again."),
                _ok);
        }
    }

    [RelayCommand]
    private async Task RemoveReportingEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return;

        bool confirmed = await ConfirmUserAsync(
            _removeEmailTitle,
            string.Format(_removeEmailMessage, email),
            _yes,
            _no);

        if (!confirmed)
            return;

        // FIX: Add error handling around EmailService calls
        try
        {
            if (EmailService.RemoveEmail(email))
            {
                ReportingEmails.Remove(email);
            }

            ShowCollectionView = ReportingEmails.Any();
        }
        catch (Exception ex)
        {
            Analytics.TrackEvent("EmailRemoveFailure",
                new Dictionary<string, string>
                {
                    { "Error", ex.Message }
                });

            await NotifyUserAsync(
                TranslationService.GetString("ErrorTitle", "Error"),
                TranslationService.GetString("EmailRemoveError", "Failed to remove email. Please try again."),
                _ok);
        }
    }

    [RelayCommand]
    private async Task RestoreDatabaseAsync()
    {
        bool confirmed = await ConfirmUserAsync(
            RestoreDatabase,
            _restoreDatabaseInfo,
            _yes,
            _no);

        if (!confirmed)
            return;

        IsLoading = true;
        await Task.Yield();

        var success = await DatabaseService.RefreshDatabaseAsync();

        IsLoading = false;

        if (success)
            await NotifyUserAsync(_restoreCompleteTitle, _restoreCompleteMessage, _ok);
        else
            await NotifyUserAsync(_restoreFailedTitle, _restoreFailedMessage, _ok);
    }

    [RelayCommand]
    private async Task ClearDataAsync()
    {
        bool confirmed = await ConfirmUserAsync(
            ClearData,
            _clearDataWarning,
            _yes,
            _no);

        if (!confirmed)
            return;

        IsLoading = true;
        await Task.Yield();

        await DatabaseService.ClearUserGiftDataAsync();

        IsLoading = false;

        await NotifyUserAsync(_dataClearedTitle, _dataClearedMessage, _ok);
    }

    // FIX: Add debounce flag to prevent rapid re-triggering
    private bool _isChangingLanguage;

    partial void OnSelectedLanguageChanged(LanguageOption? value)
    {
        if (value == null || _isChangingLanguage)
            return;

        // HIGH-1 FIX: Do NOT use fire-and-forget async
        // Dispatch async work with exception handling
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            _isChangingLanguage = true;

            try
            {
                await TranslationService.SetLanguageByCodeAsync(value.CodeOption);
                
                // Update UI after language change completes
                LanguageOptions = TranslationService.GetLanguageOptions();
                LanguageTitle = TranslationService.CurrentLanguageDisplayName;
                ShowLanguagePicker = false;
            }
            catch (Exception ex)
            {
                Analytics.TrackEvent("LanguageChangeFailure",
                    new Dictionary<string, string>
                    {
                        { "Error", ex.Message },
                        { "Language", value.CodeOption }
                    });
                
                // Keep picker open on error so user can retry
                ShowLanguagePicker = true;
            }
            finally
            {
                _isChangingLanguage = false;
            }
        });
    }

    partial void OnDebugOptionsEnabledChanged(bool value)
    {
        Preferences.Set(AppConstants.DebugOptionsEnabledKey, value);
    }

    partial void OnAllowUnansweredQuestionsChanged(bool value)
    {
        Preferences.Set(AppConstants.DebugAllowUnansweredQuestionsKey, value);
    }

    partial void OnTotalTopicsChanged(int value)
    {
        // FIX: Add bounds validation before saving
        var clampedValue = Math.Clamp(value, 0, 50);
        Preferences.Set(AppConstants.DebugTotalTopicsKey, clampedValue);

        TotalQuestions = TotalTopics * TopicLimit;
    }

    partial void OnTopicLimitChanged(int value)
    {
        // FIX: Add bounds validation before saving
        var clampedValue = Math.Clamp(value, 0, 50);
        Preferences.Set(AppConstants.DebugQuestionsPerTopicKey, clampedValue);

        TotalQuestions = TotalTopics * TopicLimit;
    }

    partial void OnTotalQuestionsChanged(int value)
    {
        Preferences.Set(AppConstants.DebugTotalQuestionsKey, TotalQuestions);
    }

    partial void OnTotalUnansweredQuestionsChanged(int value)
    {
        // FIX: Add bounds validation before saving
        var clampedValue = Math.Clamp(value, 0, 100);
        Preferences.Set(AppConstants.DebugTotalUnansweredQuestionsKey, clampedValue);
    }
}
