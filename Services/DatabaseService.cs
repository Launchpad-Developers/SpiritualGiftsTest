using CommunityToolkit.Mvvm.Messaging;
using SpiritualGiftsSurvey.Enums;
using SpiritualGiftsSurvey.Messages;
using SpiritualGiftsSurvey.Models;
using SQLite;
using System.Diagnostics;

namespace SpiritualGiftsSurvey.Services;

public interface IDatabaseService
{
    Task<Translation?> GetTranslationByCodeAsync(string languageCode);
    Guid GetTranslationGuid(string languageCode);
    List<LanguageOption> GetLanguageOptions(string languageCode);
    Task<List<Question>> GetQuestionsAsync(string languageCode);
    Task<int> GetQuestionsCountAsync(string languageCode);
    Task SaveUserGiftResultAsync(SurveyResult result);
    Task<List<GiftDescription>> GetGiftDescriptionsAsync(string languageCode);
    GiftDescription? GetGiftDescription(string languageCode, Gifts gift);
    GiftDescription? GetGiftDescription(Guid giftDescriptionGuid);
    Task<List<Verse>> GetVersesAsync(Guid giftDescriptionGuid);
    List<AppString> GetAppStrings(string languageCode);
    DatabaseInfo? GetDatabaseInfo();
    Task<bool> RefreshDatabaseAsync(RootModel? rootModel = null);
    List<SurveyResult> GetAllUserGiftResults();
    Task ClearUserGiftDataAsync();
    List<Reflection> GetReflections(string languageCode);
}

public class DatabaseService : IDatabaseService
{
    private readonly IDeviceStorageService _deviceStorage;
    private readonly IUrlService _urlService;

    public DatabaseService(IDeviceStorageService deviceStorage, IUrlService urlService)
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
        return conn.Table<Translation>().FirstOrDefault(t => t.Code == languageCode);
    }

    public List<AppString> GetAppStrings(string languageCode)
    {
        using var conn = new SQLiteConnection(DatabasePath);

        var translation = conn.Table<Translation>().FirstOrDefault(t => t.Code == languageCode);
        if (translation == null) return new List<AppString>();

        return conn.Table<AppString>()
                   .Where(x => x.TranslationGuid == translation.TranslationGuid)
                   .ToList();
    }

    public List<LanguageOption> GetLanguageOptions(string languageCode)
    {
        using var conn = new SQLiteConnection(DatabasePath);

        var translation = conn.Table<Translation>().FirstOrDefault(t => t.Code == languageCode);
        if (translation == null) return new List<LanguageOption>();

        return conn.Table<LanguageOption>()
                   .Where(x => x.TranslationGuid == translation.TranslationGuid)
                   .ToList();
    }

    //public List<Question> GetQuestions(string languageCode)
    //{
    //    using var conn = new SQLiteConnection(DatabasePath);

    //    var translation = conn.Table<Translation>().FirstOrDefault(t => t.Code == languageCode);
    //    if (translation == null) return new List<Question>();

    //    return conn.Table<Question>()
    //               .Where(x => x.TranslationGuid == translation.TranslationGuid)
    //               .ToList();
    //}

    public async Task<List<Question>> GetQuestionsAsync(string languageCode)
    {
        var conn = GetAsyncConnection();
        var translation = await conn.Table<Translation>()
                                    .FirstOrDefaultAsync(t => t.Code == languageCode);
        if (translation == null) return new List<Question>();

        return await conn.Table<Question>()
                         .Where(x => x.TranslationGuid == translation.TranslationGuid)
                         .ToListAsync();
    }

    //public List<GiftDescription> GetGiftDescriptions(string languageCode)
    //{
    //    using var conn = new SQLiteConnection(DatabasePath);

    //    var translation = conn.Table<Translation>().FirstOrDefault(t => t.Code == languageCode);
    //    if (translation == null) return new List<GiftDescription>();

    //    return conn.Table<GiftDescription>()
    //               .Where(x => x.TranslationGuid == translation.TranslationGuid)
    //               .ToList();
    //}

    public async Task<List<GiftDescription>> GetGiftDescriptionsAsync(string languageCode)
    {
        var conn = new SQLiteAsyncConnection(DatabasePath);

        var translation = await conn.Table<Translation>()
                                    .FirstOrDefaultAsync(t => t.Code == languageCode);

        if (translation == null)
            return new List<GiftDescription>();

        return await conn.Table<GiftDescription>()
                         .Where(x => x.TranslationGuid == translation.TranslationGuid)
                         .ToListAsync();
    }

    public GiftDescription? GetGiftDescription(string languageCode, Gifts gift)
    {
        using var conn = new SQLiteConnection(DatabasePath);

        var translation = conn.Table<Translation>().FirstOrDefault(t => t.Code == languageCode);
        if (translation == null) return null;

        return conn.Table<GiftDescription>()
                   .FirstOrDefault(x => x.TranslationGuid == translation.TranslationGuid && x.Gift == gift);
    }

    public GiftDescription? GetGiftDescription(Guid giftDescriptionGuid)
    {
        using var conn = new SQLiteConnection(DatabasePath);

        return conn.Table<GiftDescription>()
                   .FirstOrDefault(x => x.GiftDescriptionGuid == giftDescriptionGuid);
    }

    //public List<Verse> GetVerses(Guid giftDescriptionGuid)
    //{
    //    using var conn = new SQLiteConnection(DatabasePath);

    //    return conn.Table<Verse>()
    //               .Where(x => x.GiftDescriptionGuid == giftDescriptionGuid)
    //               .ToList();
    //}

    public async Task<List<Verse>> GetVersesAsync(Guid giftDescriptionGuid)
    {
        var conn = new SQLiteAsyncConnection(DatabasePath);

        return await conn.Table<Verse>()
                         .Where(x => x.GiftDescriptionGuid == giftDescriptionGuid)
                         .ToListAsync();
    }

    public Guid GetTranslationGuid(string languageCode)
    {
        using var conn = new SQLiteConnection(DatabasePath);

        return conn.Table<Translation>()
                   .FirstOrDefault(t => t.Code == languageCode)?.TranslationGuid ?? Guid.Empty;
    }

    //public int GetQuestionsCount(string languageCode)
    //{
    //    using var conn = new SQLiteConnection(DatabasePath);
    //    var translation = conn.Table<Translation>().FirstOrDefault(t => t.Code == languageCode);
    //    if (translation == null) return 0;

    //    return conn.Table<Question>()
    //               .Count(q => q.TranslationGuid == translation.TranslationGuid);
    //}

    public async Task<int> GetQuestionsCountAsync(string languageCode)
    {
        var conn = new SQLiteAsyncConnection(DatabasePath);

        var translation = await conn.Table<Translation>()
                                    .FirstOrDefaultAsync(t => t.Code == languageCode);

        if (translation == null)
            return 0;

        return await conn.Table<Question>()
                         .Where(q => q.TranslationGuid == translation.TranslationGuid)
                         .CountAsync();
    }

    public DatabaseInfo? GetDatabaseInfo()
    {
        try
        {
            using var conn = new SQLiteConnection(DatabasePath);
            return conn.Table<DatabaseInfo>().OrderByDescending(x => x.Id).FirstOrDefault();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error fetching DatabaseInfo: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> RefreshDatabaseAsync(RootModel? rootModel = null)
    {
        if (rootModel == null)
        {
            var result = await _urlService.GetFullDatabaseAsync();
            if (!result.IsSuccess || result.Value is null)
                return false;

            rootModel = result.Value;
        }

        using var conn = new SQLiteConnection(DatabasePath);

        // App data
        conn.CreateTable<DatabaseInfo>();
        conn.CreateTable<Translation>();
        conn.CreateTable<AppString>();
        conn.CreateTable<LanguageOption>();
        conn.CreateTable<Question>();
        conn.CreateTable<GiftDescription>();
        conn.CreateTable<Reflection>();
        conn.CreateTable<Verse>();
        conn.CreateTable<SurveyResult>();
        conn.CreateTable<UserGiftScore>();

        // Clear app data only
        conn.DeleteAll<DatabaseInfo>();
        conn.DeleteAll<Translation>();
        conn.DeleteAll<AppString>();
        conn.DeleteAll<LanguageOption>();
        conn.DeleteAll<Question>();
        conn.DeleteAll<GiftDescription>();
        conn.DeleteAll<Reflection>();
        conn.DeleteAll<Verse>();

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

                var match = translation.GiftDescriptions.FirstOrDefault(gd => gd.Gift == question.Gift);
                if (match != null)
                {
                    question.GiftDescriptionGuid = match.GiftDescriptionGuid;
                }

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
                        conn.Insert(verse);
                    }
                }
            }
        }

        WeakReferenceMessenger.Default.Send(new LanguageChangedMessage(true));
        return true;
    }

    public async Task SaveUserGiftResultAsync(SurveyResult result)
    {
        using var conn = new SQLiteConnection(DatabasePath);
        conn.CreateTable<SurveyResult>();
        conn.CreateTable<UserGiftScore>();

        conn.Insert(result);

        foreach (var score in result.Scores)
        {
            score.UserGiftResultGuid = result.UserGiftResultGuid;
            conn.Insert(score);
        }

        await Task.CompletedTask;
    }

    public List<SurveyResult> GetAllUserGiftResults()
    {
        using var conn = new SQLiteConnection(DatabasePath);
        conn.CreateTable<SurveyResult>();

        return conn.Table<SurveyResult>().OrderByDescending(x => x.DateTaken).ToList();
    }

    public async Task ClearUserGiftDataAsync()
    {
        using var conn = new SQLiteConnection(DatabasePath);
        conn.CreateTable<SurveyResult>();
        conn.CreateTable<UserGiftScore>();

        conn.DeleteAll<SurveyResult>();
        conn.DeleteAll<UserGiftScore>();

        await Task.CompletedTask;
    }
    public List<Reflection> GetReflections(string languageCode)
    {
        using var conn = new SQLiteConnection(DatabasePath);

        var translation = conn.Table<Translation>().FirstOrDefault(t => t.Code == languageCode);
        if (translation == null)
            return new List<Reflection>();

        return conn.Table<Reflection>()
                   .Where(r => r.TranslationGuid == translation.TranslationGuid)
                   .OrderBy(r => r.Number)
                   .ToList();
    }

    private async Task<bool> EnsureDatabaseUpToDate()
    {
        // 1️. If the DB file does not exist, fetch fresh data
        if (!File.Exists(DatabasePath))
        {
            return await RefreshDatabaseAsync();
        }

        using var conn = new SQLiteConnection(DatabasePath);

        // 2️. If the DatabaseInfo table does not exist, fetch fresh data
        var tableInfo = conn.GetTableInfo(nameof(DatabaseInfo));
        if (tableInfo == null || tableInfo.Count == 0)
        {
            return await RefreshDatabaseAsync();
        }

        // 3. Get local version
        var localInfo = conn.Table<DatabaseInfo>().FirstOrDefault();

        // 4️. Get remote version only
        var remoteVersionResult = await _urlService.GetRemoteDatabaseVersionAsync();
        if (!remoteVersionResult.IsSuccess)
            return false;

        var remoteVersion = remoteVersionResult.Value;

        if (localInfo == null || remoteVersion > localInfo.Version)
        {
            return await RefreshDatabaseAsync();
        }

        return true;
    }
    private SQLiteAsyncConnection GetAsyncConnection()
    {
        return new SQLiteAsyncConnection(DatabasePath);
    }
}
