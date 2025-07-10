using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpiritualGiftsSurvey.Helpers;
using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Services;
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
        Initialize();
    }

    [ObservableProperty]
    private string header = string.Empty;

    [ObservableProperty]
    private List<LanguageOption> languageOptions = new();

    [ObservableProperty]
    private LanguageOption? selectedLanguage;

    [ObservableProperty]
    private string languageTitle = "English";

    [ObservableProperty]
    private string newReportingEmail = string.Empty;

    [ObservableProperty]
    private string restoreDatabase = string.Empty;

    [ObservableProperty]
    private string clearData = string.Empty;

    private string _yes = string.Empty;
    private string _no = string.Empty;
    private string _restoreDatabaseInfo = string.Empty;
    private string _clearDataWarning = string.Empty;

    public ObservableCollection<string> ReportingEmails { get; private set; } = new();


    [RelayCommand]
    private async Task AddNewReportingEmailAsync()
    {
        if (string.IsNullOrWhiteSpace(NewReportingEmail))
            return;

        var trimmedEmail = NewReportingEmail.Trim();

        if (!PageHelper.IsValidEmail(trimmedEmail))
        {
            await NotifyUserAsync("Invalid Email", "Please enter a valid email address.", "OK");
            return;
        }

        if (!ReportingEmails.Contains(trimmedEmail, StringComparer.OrdinalIgnoreCase))
        {
            ReportingEmails.Add(trimmedEmail);
            SaveEmails();
        }

        NewReportingEmail = string.Empty;
    }

    [RelayCommand]
    private void RemoveReportingEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return;

        if (ReportingEmails.Contains(email))
        {
            ReportingEmails.Remove(email);
            SaveEmails();
        }
    }

    partial void OnSelectedLanguageChanged(LanguageOption? value)
    {
        if (value == null)
            return;

        _ = TranslationService.SetLanguageByCodeAsync(value.CodeOption);

        // If your LanguageOptions might change display names, refresh them:
        LanguageOptions = TranslationService.GetLanguageOptions();
        LanguageTitle = TranslationService.CurrentLanguageDisplayName;
    }

    public void Initialize()
    {
        LoadEmails();

        FlowDirection = TranslationService.FlowDirection;

        PageTopic = TranslationService.GetString("Settings", "Settings");
        LanguageTitle = TranslationService.CurrentLanguageDisplayName;
        LanguageOptions = TranslationService.GetLanguageOptions();

        var currentCode = TranslationService.CurrentLanguageCode;
        SelectedLanguage = LanguageOptions.FirstOrDefault(lo => lo.CodeOption == currentCode);
        
        LoadingText = TranslationService.GetString("Loading", "Loading");

        RestoreDatabase = TranslationService.GetString("RestoreDatabase", "Restore Database");
        _restoreDatabaseInfo = TranslationService.GetString("RestoreDatabaseInfo", "This action will restore the local database to it's most current form. Proceed?");
        ClearData = TranslationService.GetString("ClearData", "Clear Data");
        _clearDataWarning = TranslationService.GetString("ClearDataWarning", "WARNING: This will clear all stored results from this device. Are you sure?");
        _yes = TranslationService.GetString("Yes", "Yes");
        _no = TranslationService.GetString("No", "No");
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
            await NotifyUserAsync("Restore Complete", "Your database has been restored.", "OK");
        else
            await NotifyUserAsync("Restore Failed", "There was an error restoring the database.", "OK");
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

        await NotifyUserAsync("Data Cleared", "Your data has been erased.", "OK");
    }

    private void SaveEmails()
    {
        Preferences.Set("ReportingEmails", JsonSerializer.Serialize(ReportingEmails));
    }

    private void LoadEmails()
    {
        var stored = Preferences.Get("ReportingEmails", null);
        if (!string.IsNullOrEmpty(stored))
        {
            ReportingEmails = JsonSerializer.Deserialize<ObservableCollection<string>>(stored)
                              ?? new ObservableCollection<string>();
        }
    }
}