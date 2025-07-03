using Plugin.Settings.Abstractions;
using Plugin.Settings;
using SpiritualGiftsTest.Interfaces;
using System.Threading.Tasks;
using SpiritualGiftsTest.Models;
using System.Collections.Generic;
using SpiritualGiftsTest.Helpers;
using System.Linq;

namespace SpiritualGiftsTest.Services;

public class TranslationService : ITranslationService
{
    private IDatabaseService _databaseService { get; }
    public TranslationService(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

	private static ISettings AppSettings => CrossSettings.Current;
    public string PrimaryLanguageCode
    {
        get => AppSettings.GetValueOrDefault(nameof(PrimaryLanguageCode), AppConstants.DefaultLanguage);
        private set => AppSettings.AddOrUpdateValue(nameof(PrimaryLanguageCode), value);
    }

    public static string PrimaryLanguageName
    {
        get => AppSettings.GetValueOrDefault(nameof(PrimaryLanguageName), AppConstants.DefaultLanguage);
        private set => AppSettings.AddOrUpdateValue(nameof(PrimaryLanguageName), value);
    }

    public string ParallelLanguageCode
    {
        get => AppSettings.GetValueOrDefault(nameof(ParallelLanguageCode), AppConstants.DefaultLanguage);
        private set => AppSettings.AddOrUpdateValue(nameof(ParallelLanguageCode), value);
    }

    public static string ParallelLanguageName
    {
        get => AppSettings.GetValueOrDefault(nameof(ParallelLanguageName), AppConstants.DefaultLanguage);
        private set => AppSettings.AddOrUpdateValue(nameof(ParallelLanguageName), value);
    }

    private TranslationModel _primaryLanguage;
    public TranslationModel PrimaryLanguage 
    { 
        get { return _primaryLanguage; }
        private set { _primaryLanguage = value; }
    }

    private TranslationModel _parallelLanguage;
    public TranslationModel ParallelLanguage
    {
        get { return _parallelLanguage; }
        private set { _parallelLanguage = value; }
    }

    private IEnumerable<TranslationOptionModel> _primaryTranslationOptions;
	public IEnumerable<TranslationOptionModel> PrimaryTranslationOptions
	{
		get { return _primaryTranslationOptions; }
		private set { _primaryTranslationOptions = value; }
    }

    private IEnumerable<TranslationOptionModel> _parallelTranslationOptions;
    public IEnumerable<TranslationOptionModel> ParallelTranslationOptions
    {
        get { return _parallelTranslationOptions; }
        private set { _parallelTranslationOptions = value; }
    }

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
        ParallelLanguageName = PrimaryTranslationOptions.Where(x => x.Code == current.Code).Select(x => x.CodeOptionTranslation).FirstOrDefault();

        return await InitializeLanguages();
    }

    public async Task<bool> InitializeLanguages()
    {         
		var language = await _databaseService.GetTranslationForCode(PrimaryLanguageCode);
		PrimaryLanguage = language;

        var parallelLanguage = await _databaseService.GetTranslationForCode(ParallelLanguageCode);
        ParallelLanguage = parallelLanguage;

        PrimaryTranslationOptions = _databaseService.GetCurrentTranslationOptions(PrimaryLanguageCode);
        ParallelTranslationOptions = _databaseService.GetCurrentTranslationOptions(ParallelLanguageCode);

        return (PrimaryLanguage != null);
    }
}
