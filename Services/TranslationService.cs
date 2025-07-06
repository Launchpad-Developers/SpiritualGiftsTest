using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SpiritualGiftsTest.Helpers;
using SpiritualGiftsTest.Messages;
using SpiritualGiftsTest.Models;

namespace SpiritualGiftsTest.Services;

public interface ITranslationService
{
    Translation Language { get; }

    IEnumerable<LanguageOption> LanguageOptions { get; }

    string CurrentLanguageCode { get; }

    string CurrentLanguageDisplayName { get; }

    Task<bool> InitializeLanguage();
    Task<bool> SetLanguageByCodeAsync(string code);
}

public partial class TranslationService : ObservableObject, ITranslationService
{
    private readonly IDatabaseService _databaseService;
    private readonly IPreferences _prefs;

    private const string DefaultLang = AppConstants.DefaultLanguage;

    public TranslationService(IDatabaseService databaseService, IPreferences preferences)
    {
        _databaseService = databaseService;
        _prefs = preferences;
    }

    [ObservableProperty]
    private string currentLanguageCode = string.Empty;

    [ObservableProperty]
    private string currentLanguageDisplayName = string.Empty;

    [ObservableProperty]
    private Translation language = new();

    [ObservableProperty]
    private IEnumerable<LanguageOption> languageOptions = Enumerable.Empty<LanguageOption>();

    partial void OnCurrentLanguageCodeChanged(string value)
    {
        Preferences.Default.Set(nameof(CurrentLanguageCode), value);
        Preferences.Default.Set(nameof(CurrentLanguageDisplayName), value);
    }
    partial void OnCurrentLanguageDisplayNameChanged(string value)
    {
        Preferences.Default.Set(nameof(CurrentLanguageDisplayName), value);
    }

    public void LoadLanguageCode()
    {
        CurrentLanguageCode = Preferences.Default.Get(nameof(CurrentLanguageCode), "EN");
        CurrentLanguageDisplayName = Preferences.Default.Get(nameof(CurrentLanguageDisplayName), "English");
    }

    public async Task<bool> InitializeLanguage()
    {
        Language = await _databaseService.GetTranslationByCodeAsync(CurrentLanguageCode) ?? new();

        LanguageOptions = await _databaseService.GetLanguageOptionsAsync(CurrentLanguageCode);

        return Language is not null && 
               LanguageOptions is not null;
    }

    public async Task<bool> SetLanguageByCodeAsync(string code)
    {
        if (string.IsNullOrEmpty(code)) return false;

        CurrentLanguageCode = code;

        if (await InitializeLanguage())
        {
            foreach (var option in LanguageOptions)
            {
                if (option.CodeOption == code)
                {
                    CurrentLanguageDisplayName = option.DisplayName;
                    WeakReferenceMessenger.Default.Send(new LanguageChangedMessage(option));
                    return true;
                }
            }
        }

        return false;
    }
}
