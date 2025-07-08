using SpiritualGiftsSurvey.Models;
using SQLite;

namespace SpiritualGiftsSurvey.Services;

public interface IDatabaseService
{
    Task<Translation?> GetTranslationByCodeAsync(string languageCode);
    Guid GetTranslationGuid(string languageCode);
    List<LanguageOption> GetLanguageOptions(string languageCode);
    List<Question> GetQuestions(string languageCode);
    List<GiftDescription> GetGiftDescriptions(string languageCode);
    List<Verse> GetVerses(Guid giftDescriptionGuid);
    List<AppString> GetAppStrings(string languageCode);
    int GetQuestionsCount(string languageCode);
}

public class DatabaseService : IDatabaseService
{
    private readonly IDeviceStorageService _deviceStorage;
    private readonly IURLService _urlService;

    public DatabaseService(IDeviceStorageService deviceStorage, IURLService urlService)
    {
        _deviceStorage = deviceStorage;
        _urlService = urlService;
    }

    private string DatabasePath => _deviceStorage.GetDatabaseFileLocation();

    public async Task<Translation?> GetTranslationByCodeAsync(string languageCode)
    {
        if (!await EnsureDatabaseUpToDate())
            return null;

        using var conn = new SQLiteConnection(DatabasePath);
        var translation = conn.Table<Translation>().FirstOrDefault(t => t.Code == languageCode);

        return translation;
    }

    public List<AppString> GetAppStrings(string languageCode)
    {
        using var conn = new SQLiteConnection(DatabasePath);

        var translation = conn.Table<Translation>()
            .FirstOrDefault(t => t.Code == languageCode);

        if (translation == null)
            return new List<AppString>();

        var appStrings = conn.Table<AppString>()
            .Where(x => x.TranslationGuid == translation.TranslationGuid)
            .ToList();

        return appStrings;
    }

    public List<LanguageOption> GetLanguageOptions(string languageCode)
    {
        using var conn = new SQLiteConnection(DatabasePath);

        var translation = conn.Table<Translation>()
            .FirstOrDefault(t => t.Code == languageCode);

        if (translation == null)
            return new List<LanguageOption>();

        return conn.Table<LanguageOption>()
            .Where(x => x.TranslationGuid == translation.TranslationGuid)
            .ToList();
    }

    public List<Question> GetQuestions(string languageCode)
    {
        using var conn = new SQLiteConnection(DatabasePath);

        var translation = conn.Table<Translation>()
            .FirstOrDefault(t => t.Code == languageCode);

        if (translation == null)
            return new List<Question>();

        return conn.Table<Question>()
            .Where(x => x.TranslationGuid == translation.TranslationGuid)
            .ToList();
    }

    public List<GiftDescription> GetGiftDescriptions(string languageCode)
    {
        using var conn = new SQLiteConnection(DatabasePath);

        var translation = conn.Table<Translation>()
            .FirstOrDefault(t => t.Code == languageCode);

        if (translation == null)
            return new List<GiftDescription>();

        return conn.Table<GiftDescription>()
            .Where(x => x.TranslationGuid == translation.TranslationGuid)
            .ToList();
    }

    public List<Verse> GetVerses(Guid giftDescriptionGuid)
    {
        using var conn = new SQLiteConnection(DatabasePath);

        return conn.Table<Verse>()
            .Where(x => x.GiftDescriptionGuid == giftDescriptionGuid)
            .ToList();
    }

    public Guid GetTranslationGuid(string languageCode)
    {
        using var conn = new SQLiteConnection(DatabasePath);

        return conn.Table<Translation>()
            .FirstOrDefault(t => t.Code == languageCode)?.TranslationGuid ?? Guid.Empty;
    }

    public int GetQuestionsCount(string languageCode)
    {
        using var conn = new SQLiteConnection(DatabasePath);
        var translation = conn.Table<Translation>().FirstOrDefault(t => t.Code == languageCode);

        if (translation == null)
            return 0;

        return conn.Table<Question>()
            .Count(q => q.TranslationGuid == translation.TranslationGuid);
    }

    private async Task<bool> EnsureDatabaseUpToDate()
    {
        if (!File.Exists(DatabasePath))
            return await RefreshDatabaseAsync();

        using var conn = new SQLiteConnection(DatabasePath);
        var localInfo = conn.Table<DatabaseInfo>().FirstOrDefault();

        var remoteResult = await _urlService.GetFullDatabaseAsync();
        if (!remoteResult.IsSuccess || remoteResult.Value is null)
            return false;

        var remoteInfo = remoteResult.Value.Database;

        if (localInfo == null || remoteInfo.Version > localInfo.Version)
            return await RefreshDatabaseAsync(remoteResult.Value);

        return true;
    }

    private async Task<bool> RefreshDatabaseAsync(RootModel? rootModel = null)
    {
        _deviceStorage.DeleteFile(DatabasePath);

        if (rootModel == null)
        {
            var result = await _urlService.GetFullDatabaseAsync();
            if (!result.IsSuccess || result.Value is null)
                return false;

            rootModel = result.Value;
        }

        using var conn = new SQLiteConnection(DatabasePath);
        conn.CreateTable<DatabaseInfo>();
        conn.CreateTable<Translation>();
        conn.CreateTable<AppString>();
        conn.CreateTable<LanguageOption>();
        conn.CreateTable<Question>();
        conn.CreateTable<GiftDescription>();
        conn.CreateTable<Reflection>();
        conn.CreateTable<Verse>();

        conn.Insert(rootModel.Database);

        foreach (var translation in rootModel.Translations)
        {
            conn.Insert(translation);

            foreach (var appString in translation.AppStrings)
            {
                appString.TranslationGuid = translation.TranslationGuid;
                conn.Insert(appString);
            }

            foreach (var languageOption in translation.LanguageOptions)
            {
                languageOption.TranslationGuid = translation.TranslationGuid;
                conn.Insert(languageOption);
            }

            foreach (var question in translation.Questions)
            {
                question.TranslationGuid = translation.TranslationGuid;
                conn.Insert(question);
            }

            foreach (var reflection in translation.Reflections)
            {
                reflection.TranslationGuid = translation.TranslationGuid;
                conn.Insert(reflection);
            }

            foreach (var giftDescription in translation.GiftDescriptions)
            {
                giftDescription.TranslationGuid = translation.TranslationGuid;
                conn.Insert(giftDescription);

                if (giftDescription.Verses != null)
                {
                    foreach (var verse in giftDescription.Verses)
                    {
                        verse.GiftDescriptionGuid = giftDescription.GiftDescriptionGuid;
                        verse.VerseText = verse.VerseText;
                        conn.Insert(verse);
                    }
                }
            }
        }

        return true;
    }
}
