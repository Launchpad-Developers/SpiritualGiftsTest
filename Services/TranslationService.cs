using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SpiritualGiftsSurvey.Messages;
using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Utilities;

namespace SpiritualGiftsSurvey.Services;

public interface ITranslationService
{
    IEnumerable<LanguageOption> LanguageOptions { get; }
    string CurrentLanguageCode { get; }
    string CurrentLanguageDisplayName { get; }
    FlowDirection FlowDirection { get; }
    int TotalQuestions { get; } 
    Task<bool> InitializeLanguage();
    Task<bool> SetLanguageByCodeAsync(string code);
    string GetString(string key, string defaultValue = "");
    List<LanguageOption> GetLanguageOptions();
}

public partial class TranslationService : ObservableObject, ITranslationService
{
    private readonly IDatabaseService _databaseService;
    private readonly IPreferences _prefs;

    private const string DefaultLangCode = AppConstants.DefaultLangCode;
    private const string DefaultLangName = AppConstants.DefaultLangName;

    private const string LangCodeKey = nameof(CurrentLanguageCode);
    private const string LangNameKey = nameof(CurrentLanguageDisplayName);

    public TranslationService(IDatabaseService databaseService, IPreferences preferences)
    {
        _databaseService = databaseService;
        _prefs = preferences;
    }

    [ObservableProperty]
    private FlowDirection flowDirection = FlowDirection.LeftToRight;

    [ObservableProperty]
    private int totalQuestions = Preferences.Get(AppConstants.DebugTotalQuestionsKey, 0);

    [ObservableProperty]
    private IEnumerable<LanguageOption> languageOptions = Enumerable.Empty<LanguageOption>();

    [ObservableProperty]
    private Dictionary<string, string> appStrings = new();

    public string CurrentLanguageCode
    {
        get => Preferences.Default.Get(LangCodeKey, DefaultLangCode);
        set
        {
            if (value != CurrentLanguageCode)
            {
                Preferences.Default.Set(LangCodeKey, value);
                OnPropertyChanged();
            }
        }
    }

    public string CurrentLanguageDisplayName
    {
        get => Preferences.Default.Get(LangNameKey, DefaultLangName);
        set
        {
            if (value != CurrentLanguageDisplayName)
            {
                Preferences.Default.Set(LangNameKey, value);
                OnPropertyChanged();
            }
        }
    }

    public async Task<bool> InitializeLanguage()
    {
        Console.WriteLine($"[TranslationService] ========================================");
        Console.WriteLine($"[TranslationService] Starting InitializeLanguage");
        Console.WriteLine($"[TranslationService] CurrentLanguageCode: {CurrentLanguageCode}");
        
        var translation = await _databaseService.GetTranslationByCodeAsync(CurrentLanguageCode);
        Console.WriteLine($"[TranslationService] Translation fetched - IsNull: {translation is null}");

        if (translation is null)
        {
            Console.WriteLine($"[TranslationService] ❌ Translation is null - returning false");
            Console.WriteLine($"[TranslationService] ========================================");
            return false;
        }

        Console.WriteLine($"[TranslationService] Translation found - Code: {translation.Code}, GUID: {translation.TranslationGuid}");
        Console.WriteLine($"[TranslationService] Fetching AppStrings...");
        
        var appStringsList = _databaseService.GetAppStrings(CurrentLanguageCode);
        Console.WriteLine($"[TranslationService] AppStrings fetched - Count: {appStringsList?.Count() ?? 0}");
        
        AppStrings = appStringsList.ToDictionary(x => x.Key, x => x.Value);
        Console.WriteLine($"[TranslationService] AppStrings dictionary created - Count: {AppStrings.Count}");
        
        // Log first 10 keys for diagnostics
        if (AppStrings.Count > 0)
        {
            Console.WriteLine($"[TranslationService] First 10 AppStrings keys:");
            foreach (var key in AppStrings.Keys.Take(10))
            {
                Console.WriteLine($"[TranslationService]   - '{key}'");
            }
        }

        Console.WriteLine($"[TranslationService] Fetching LanguageOptions...");
        LanguageOptions = _databaseService.GetLanguageOptions(CurrentLanguageCode);
        Console.WriteLine($"[TranslationService] LanguageOptions fetched - Count: {LanguageOptions?.Count() ?? 0}");
        
        FlowDirection = ParseFlowDirection(translation.FlowDirection);
        Console.WriteLine($"[TranslationService] FlowDirection: {FlowDirection}");
        
        TotalQuestions = await _databaseService.GetQuestionsCountAsync(CurrentLanguageCode);
        Console.WriteLine($"[TranslationService] TotalQuestions: {TotalQuestions}");

        bool hasAppStrings = AppStrings.Any();
        bool hasLanguageOptions = LanguageOptions.Any();
        Console.WriteLine($"[TranslationService] AppStrings.Any(): {hasAppStrings}");
        Console.WriteLine($"[TranslationService] LanguageOptions.Any(): {hasLanguageOptions}");
        
        bool result = hasAppStrings && hasLanguageOptions;
        Console.WriteLine($"[TranslationService] Returning: {result}");
        Console.WriteLine($"[TranslationService] ========================================");
        
        return result;
    }

    public async Task<bool> SetLanguageByCodeAsync(string code)
    {
        if (string.IsNullOrEmpty(code) ||
            CurrentLanguageCode == code) 
            return false;

        CurrentLanguageCode = code;

        if (await InitializeLanguage())
        {
            // Set display name if available
            CurrentLanguageDisplayName = LanguageOptions
                .FirstOrDefault(lo => lo.CodeOption == code)?.DisplayName ?? DefaultLangName;

            WeakReferenceMessenger.Default.Send(new LanguageChangedMessage(!string.IsNullOrEmpty(CurrentLanguageDisplayName)));
            return true;
        }

        return false;
    }

    public string GetString(string key, string defaultValue = "")
    {
        Console.WriteLine($"[TranslationService.GetString] Requesting key: '{key}'");
        Console.WriteLine($"[TranslationService.GetString] AppStrings.Count: {AppStrings.Count}");
        
        bool found = AppStrings.TryGetValue(key, out var value);
        Console.WriteLine($"[TranslationService.GetString] Key found: {found}");
        
        if (found)
        {
            Console.WriteLine($"[TranslationService.GetString] Value length: {value?.Length ?? 0} chars");
            return value ?? defaultValue;
        }
        
        Console.WriteLine($"[TranslationService.GetString] Returning default: '{defaultValue?.Substring(0, Math.Min(50, defaultValue?.Length ?? 0))}...'");
        return defaultValue;
    }

    private static FlowDirection ParseFlowDirection(string flowDirectionString)
    {
        return flowDirectionString?.ToUpperInvariant() == "RTL"
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;
    }

    public List<LanguageOption> GetLanguageOptions()
    {
        if (LanguageOptions != null && LanguageOptions.Any())
            return LanguageOptions.ToList();

        var options = _databaseService.GetLanguageOptions(CurrentLanguageCode);

        return options?.ToList() ?? new List<LanguageOption>();
    }
}
