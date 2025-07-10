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
    private int totalQuestions = AppConstants.TotalQuestions;

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
        // Get base translation info — only to read metadata like FlowDirection
        var translation = await _databaseService.GetTranslationByCodeAsync(CurrentLanguageCode);

        // Defensive fallback if null
        if (translation is null)
            return false;

        // Load AppStrings as Dictionary
        AppStrings = _databaseService
            .GetAppStrings(CurrentLanguageCode)
            .ToDictionary(x => x.Key, x => x.Value);

        // Load available language options
        LanguageOptions = _databaseService.GetLanguageOptions(CurrentLanguageCode);

        // Set FlowDirection based on DB value or default to LTR
        FlowDirection = ParseFlowDirection(translation.FlowDirection);

        // Load total questions
        TotalQuestions = _databaseService.GetQuestionsCount(CurrentLanguageCode);

        // Done: returns success only if you actually have strings + options
        return AppStrings.Any() && LanguageOptions.Any();
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
        => AppStrings.TryGetValue(key, out var value) ? value : defaultValue;

    private static FlowDirection ParseFlowDirection(string flowDirectionString)
    {
        return flowDirectionString?.ToUpperInvariant() == "RTL"
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;
    }

    public List<LanguageOption> GetLanguageOptions()
    {
        // Defensive fallback in case this is called before Init
        if (LanguageOptions != null && LanguageOptions.Any())
            return LanguageOptions.ToList();

        var options = _databaseService.GetLanguageOptions(CurrentLanguageCode);

        return options?.ToList() ?? new List<LanguageOption>();
    }
}
