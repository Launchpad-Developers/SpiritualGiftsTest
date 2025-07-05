using CommunityToolkit.Mvvm.ComponentModel;
using SpiritualGiftsTest.Helpers;
using SpiritualGiftsTest.Models;

namespace SpiritualGiftsTest.Services;

public interface ITranslationService
{
    TranslationModel PrimaryLanguage { get; }
    TranslationModel ParallelLanguage { get; }

    IEnumerable<TranslationOptionModel> PrimaryTranslationOptions { get; }
    IEnumerable<TranslationOptionModel> ParallelTranslationOptions { get; }

    string PrimaryLanguageCode { get; }
    string ParallelLanguageCode { get; }

    /// <summary>
    /// Initializes the translation.
    /// This should only be called when app first starts or if the Primary language changes.
    /// </summary>
    /// <returns>True if translation could be set, false otherwise.</returns>
    Task<bool> InitializeLanguages();

    /// <summary>
    /// Sets the current translation.
    /// </summary>
    /// <returns>True if translation could be set, false otherwise.</returns>        
    Task<bool> SetPrimaryLanguageForCode(TranslationOptionModel current);

    /// <summary>
    /// Sets the current translation.
    /// </summary>
    /// <returns>True if translation could be set, false otherwise.</returns>        
    Task<bool> SetParallelLanguageForCode(TranslationOptionModel current);
}

public partial class TranslationService : ObservableObject, ITranslationService
{
    private IDatabaseService _databaseService { get; }
    private readonly IPreferences _prefs;

    private const string DefaultLang = AppConstants.DefaultLanguage;

    public TranslationService(
        IDatabaseService databaseService,
        IPreferences preferences)
    {
        _databaseService = databaseService;
        _prefs = preferences;
    }

    public string PrimaryLanguageCode
    {
        get => _prefs.Get(nameof(PrimaryLanguageCode), DefaultLang);
        private set => _prefs.Set(nameof(PrimaryLanguageCode), value);
    }

    public string PrimaryLanguageName
    {
        get => _prefs.Get(nameof(PrimaryLanguageName), DefaultLang);
        private set => _prefs.Set(nameof(PrimaryLanguageName), value);
    }

    public string ParallelLanguageCode
    {
        get => _prefs.Get(nameof(ParallelLanguageCode), DefaultLang);
        private set => _prefs.Set(nameof(ParallelLanguageCode), value);
    }

    public string ParallelLanguageName
    {
        get => _prefs.Get(nameof(ParallelLanguageName), DefaultLang);
        private set => _prefs.Set(nameof(ParallelLanguageName), value);
    }


    [ObservableProperty]
    private TranslationModel primaryLanguage = new();

    [ObservableProperty]
    private TranslationModel parallelLanguage = new();

    [ObservableProperty]
    private IEnumerable<TranslationOptionModel> primaryTranslationOptions = new List<TranslationOptionModel>();

    [ObservableProperty]
    private IEnumerable<TranslationOptionModel> parallelTranslationOptions = new List<TranslationOptionModel>();

    public async Task<bool> SetPrimaryLanguageForCode(TranslationOptionModel current)
    {
        if (current == null) return false;

        PrimaryLanguageCode = current.CodeOption;
        PrimaryLanguageName = current.CodeOptionTranslation;
        return await InitializeLanguages();
    }

    public async Task<bool> SetParallelLanguageForCode(TranslationOptionModel current)
    {
        if (current == null) return false;

        ParallelLanguageCode = current.CodeOption;
        // find the translation text from the primary options
        ParallelLanguageName = PrimaryTranslationOptions
            .FirstOrDefault(x => x.Code == current.Code)?
            .CodeOptionTranslation
            ?? DefaultLang;

        return await InitializeLanguages();
    }

    public async Task<bool> InitializeLanguages()
    {
        var primaryLanguage = await _databaseService.GetTranslationForCode(PrimaryLanguageCode);
        var parallelLanguage = await _databaseService.GetTranslationForCode(ParallelLanguageCode);

        if (primaryLanguage == null || parallelLanguage == null)
        {
            return false; // Handle null cases explicitly
        }

        PrimaryLanguage = primaryLanguage;
        ParallelLanguage = parallelLanguage;

        PrimaryTranslationOptions = _databaseService.GetCurrentTranslationOptions(PrimaryLanguageCode);
        ParallelTranslationOptions = _databaseService.GetCurrentTranslationOptions(ParallelLanguageCode);

        return true;
    }
}
