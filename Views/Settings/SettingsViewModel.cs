using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpiritualGiftsSurvey.Helpers;
using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Utilities;
using SpiritualGiftsSurvey.Views.Shared;
using System.Collections.ObjectModel;
using System.Runtime.Versioning;
using System.Text.Json;

namespace SpiritualGiftsSurvey.Views.Settings;

[SupportedOSPlatform("android")]
[SupportedOSPlatform("ios")]
public partial class SettingsViewModel : BaseViewModel
{
    public SettingsViewModel(
        IAggregatedServices aggregatedServices,
        IPreferences preferences) 
        : base(aggregatedServices, preferences)
    {

    }

    [ObservableProperty]
    private string header = string.Empty;

    [ObservableProperty]
    private List<LanguageOption> languageOptions = new();

    [ObservableProperty]
    private LanguageOption? selectedLanguage;

    [ObservableProperty]
    private string languageTitle = string.Empty;

    [ObservableProperty]
    private string currentLanguageTitle = string.Empty;

    [ObservableProperty]
    private string newReportingEmail = string.Empty;

    [ObservableProperty]
    private string restoreDatabase = string.Empty;

    [ObservableProperty]
    private string clearData = string.Empty;

    [ObservableProperty]
    private string reportingEmailsTitle = string.Empty;

    [ObservableProperty]
    private string addReportingEmailPlaceholder = string.Empty;

    [ObservableProperty]
    private bool showLanguagePicker;

    [ObservableProperty]
    private bool showCollectionView;

    [ObservableProperty]
    private Color addReportingEmailTextColor =
                (Application.Current?.Resources.TryGetValue("Black", out var value) == true && value is Color color)
                    ? color
                    : Colors.Black;

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

    public ObservableCollection<string> ReportingEmails { get; private set; } = new();


    public override void InitAsync()
    {
        if (!RequiresInitialzation)
            return;

        RequiresInitialzation = false;

        LoadEmails();

        FlowDirection = TranslationService.FlowDirection;

        PageTopic = TranslationService.GetString("Settings", "Settings");
        LanguageTitle = TranslationService.CurrentLanguageDisplayName;
        LanguageOptions = TranslationService.GetLanguageOptions();
        CurrentLanguageTitle = TranslationService.GetString("Language", "Language");
        ReportingEmailsTitle = TranslationService.GetString("ReportingEmailsTitle", "Reporting Emails");
        AddReportingEmailPlaceholder = TranslationService.GetString("AddReportingEmailPlaceholder", "Add reporting email");

        var currentCode = TranslationService.CurrentLanguageCode;
        SelectedLanguage = LanguageOptions.FirstOrDefault(lo => lo.CodeOption == currentCode);

        LoadingText = TranslationService.GetString("Loading", "Loading");

        _yes = TranslationService.GetString("Yes", "Yes");
        _no = TranslationService.GetString("No", "No");
        _ok = TranslationService.GetString("OK", "OK");

        RestoreDatabase = TranslationService.GetString("RestoreDatabase", "Restore Database");
        _restoreDatabaseInfo = TranslationService.GetString("RestoreDatabaseInfo", "This action will restore the local database to it's most current form. Proceed?");
        _restoreCompleteTitle = TranslationService.GetString(_restoreCompleteTitle, "Restore Complete");
        _restoreCompleteMessage = TranslationService.GetString(_restoreCompleteMessage, "Your database has been restored. Please restart the app.");
        _restoreFailedTitle = TranslationService.GetString(_restoreFailedTitle, "Restore Failed");
        _restoreFailedMessage = TranslationService.GetString(_restoreFailedMessage, "There was an error restoring the database.");

        ClearData = TranslationService.GetString("ClearData", "Clear Data");
        _clearDataWarning = TranslationService.GetString("ClearDataWarning", "WARNING: This will clear all stored results from this device. Are you sure?");
        _dataClearedTitle = TranslationService.GetString(_dataClearedTitle, "Data Cleared");
        _dataClearedMessage = TranslationService.GetString(_dataClearedMessage, "Your data has been erased.");

        _invalidEmailTitle = TranslationService.GetString("InvalidEmailTitle", "Invalid Email");
        _invalidEmailMessage = TranslationService.GetString("InvalidEmailMessage", "Please enter a valid email address.");
        
        _removeEmailTitle = TranslationService.GetString("RemoveEmailTitle", "Remove Email");
        _removeEmailMessage = TranslationService.GetString("RemoveEmailMessage", "Remove {0} from the reporting list?");
    }

    private bool _emailIsError;
    public void ResetAddReportingEmailEntry()
    {
        if (!_emailIsError) return;

        AddReportingEmailTextColor =
                (Application.Current?.Resources.TryGetValue("Black", out var value) == true && value is Color color)
                    ? color
                    : Colors.Black;
    }

    [RelayCommand]
    private async Task AddNewReportingEmailAsync()
    {
        if (string.IsNullOrWhiteSpace(NewReportingEmail))
            return;

        var trimmedEmail = NewReportingEmail.Trim();

        if (!PageHelper.IsValidEmail(trimmedEmail))
        {
            AddReportingEmailTextColor =
                (Application.Current?.Resources.TryGetValue("DangerRed", out var value) == true && value is Color color)
                    ? color
                    : Colors.Black;

            await NotifyUserAsync(_invalidEmailTitle, _invalidEmailMessage, _ok);
            return;
        }

        if (!ReportingEmails.Contains(trimmedEmail, StringComparer.OrdinalIgnoreCase))
        {
            ReportingEmails.Add(trimmedEmail);
            SaveEmails();
        }

        NewReportingEmail = string.Empty;

        ShowCollectionView = ReportingEmails.Any();
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

        if (ReportingEmails.Contains(email))
        {
            ReportingEmails.Remove(email);
            SaveEmails();
        }

        ShowCollectionView = ReportingEmails.Any();
    }

    partial void OnSelectedLanguageChanged(LanguageOption? value)
    {
        if (value == null)
            return;

        _ = TranslationService.SetLanguageByCodeAsync(value.CodeOption);

        // If your LanguageOptions might change display names, refresh them:
        LanguageOptions = TranslationService.GetLanguageOptions();
        LanguageTitle = TranslationService.CurrentLanguageDisplayName;
        ShowLanguagePicker = false;
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

        await DatabaseService.ClearUserGiftDataAsync();

        IsLoading = false;

        await NotifyUserAsync(_dataClearedTitle, _dataClearedMessage, _ok);
    }

    private void SaveEmails()
    {
        Preferences.Set(AppConstants.ReportingEmailsKey, JsonSerializer.Serialize(ReportingEmails));
    }

    private void LoadEmails()
    {
        var stored = Preferences.Get(AppConstants.ReportingEmailsKey, null);
        if (!string.IsNullOrEmpty(stored))
        {
            var list = JsonSerializer.Deserialize<List<string>>(stored) ?? new List<string>();

            ReportingEmails.Clear();

            foreach (var email in list)
            {
                ReportingEmails.Add(email);
            }
        }

        ShowCollectionView = ReportingEmails.Any();
    }
}