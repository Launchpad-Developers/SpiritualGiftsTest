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
        return conn.Table<Translation>().FirstOrDefault(t => t.Code.ToLower() == languageCode.ToLower());
    }

    public List<AppString> GetAppStrings(string languageCode)
    {
        Console.WriteLine($"[DatabaseService.GetAppStrings] Called with languageCode: '{languageCode}'");
        
        using var conn = new SQLiteConnection(DatabasePath);

        var translation = conn.Table<Translation>().FirstOrDefault(t => t.Code.ToLower() == languageCode.ToLower());
        Console.WriteLine($"[DatabaseService.GetAppStrings] Translation found: {translation != null}");
        
        if (translation == null)
        {
            Console.WriteLine($"[DatabaseService.GetAppStrings] Returning empty list");
            return new List<AppString>();
        }

        Console.WriteLine($"[DatabaseService.GetAppStrings] Translation GUID: {translation.TranslationGuid}");
        var appStrings = conn.Table<AppString>()
                   .Where(x => x.TranslationGuid == translation.TranslationGuid)
                   .ToList();
        Console.WriteLine($"[DatabaseService.GetAppStrings] AppStrings count: {appStrings.Count}");
        
        return appStrings;
    }

    public List<LanguageOption> GetLanguageOptions(string languageCode)
    {
        Console.WriteLine($"[DatabaseService.GetLanguageOptions] Called with languageCode: '{languageCode}'");
        
        using var conn = new SQLiteConnection(DatabasePath);

        var translation = conn.Table<Translation>().FirstOrDefault(t => t.Code.ToLower() == languageCode.ToLower());
        Console.WriteLine($"[DatabaseService.GetLanguageOptions] Translation found: {translation != null}");
        
        if (translation == null)
        {
            Console.WriteLine($"[DatabaseService.GetLanguageOptions] Returning empty list");
            return new List<LanguageOption>();
        }

        Console.WriteLine($"[DatabaseService.GetLanguageOptions] Translation GUID: {translation.TranslationGuid}");
        var languageOptions = conn.Table<LanguageOption>()
                   .Where(x => x.TranslationGuid == translation.TranslationGuid)
                   .ToList();
        Console.WriteLine($"[DatabaseService.GetLanguageOptions] LanguageOptions count: {languageOptions.Count}");
        
        return languageOptions;
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
        // Defensive check: return empty list if database doesn't exist
        if (!File.Exists(DatabasePath))
            return new List<Question>();

        var conn = GetAsyncConnection();
        try
        {
            var translation = await conn.Table<Translation>()
                                        .FirstOrDefaultAsync(t => t.Code.ToLower() == languageCode.ToLower());
            if (translation == null) return new List<Question>();

            return await conn.Table<Question>()
                             .Where(x => x.TranslationGuid == translation.TranslationGuid)
                             .ToListAsync();
        }
        finally
        {
            await conn.CloseAsync();
        }
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
        // Defensive check: return empty list if database doesn't exist
        if (!File.Exists(DatabasePath))
            return new List<GiftDescription>();

        var conn = new SQLiteAsyncConnection(DatabasePath);
        try
        {
            var translation = await conn.Table<Translation>()
                                        .FirstOrDefaultAsync(t => t.Code.ToLower() == languageCode.ToLower());

            if (translation == null)
                return new List<GiftDescription>();

            return await conn.Table<GiftDescription>()
                             .Where(x => x.TranslationGuid == translation.TranslationGuid)
                             .ToListAsync();
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public GiftDescription? GetGiftDescription(string languageCode, Gifts gift)
    {
        using var conn = new SQLiteConnection(DatabasePath);

        var translation = conn.Table<Translation>().FirstOrDefault(t => t.Code.ToLower() == languageCode.ToLower());
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
                   .FirstOrDefault(t => t.Code.ToLower() == languageCode.ToLower())?.TranslationGuid ?? Guid.Empty;
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
        try
        {
            var translation = await conn.Table<Translation>()
                                        .FirstOrDefaultAsync(t => t.Code.ToLower() == languageCode.ToLower());

            if (translation == null)
                return 0;

            return await conn.Table<Question>()
                            .Where(q => q.TranslationGuid == translation.TranslationGuid)
                            .CountAsync();
        }
        finally
        {
            await conn.CloseAsync();
        }
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
        Console.WriteLine($"[DatabaseService] ========================================");
        Console.WriteLine($"[DatabaseService] Starting RefreshDatabaseAsync");
        Console.WriteLine($"[DatabaseService] RootModel provided: {rootModel != null}");
        Console.WriteLine($"[DatabaseService] Database Path: {DatabasePath}");
        
        if (rootModel == null)
        {
            Console.WriteLine($"[DatabaseService] Fetching database from Firebase...");
            var result = await _urlService.GetFullDatabaseAsync();
            
            Console.WriteLine($"[DatabaseService] GetFullDatabaseAsync result - IsSuccess: {result.IsSuccess}");
            
            if (!result.IsSuccess || result.Value is null)
            {
                if (result.Error != null)
                {
                    Console.WriteLine($"[DatabaseService] ❌ Failed to fetch database");
                    Console.WriteLine($"[DatabaseService] Error Title: {result.Error.ExceptionTitle}");
                    Console.WriteLine($"[DatabaseService] Error Message: {result.Error.Message}");
                }
                else
                {
                    Console.WriteLine($"[DatabaseService] ❌ GetFullDatabaseAsync returned null value");
                }
                return false;
            }

            Console.WriteLine($"[DatabaseService] ✅ Database fetched successfully");
            rootModel = result.Value;
        }

        try
        {
            Console.WriteLine($"[DatabaseService] Opening SQLite connection...");
            using var conn = new SQLiteConnection(DatabasePath);
            Console.WriteLine($"[DatabaseService] ✅ SQLite connection opened");

            // App data
            Console.WriteLine($"[DatabaseService] Creating tables...");
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
            conn.CreateTable<SurveyProgress>(); // Survey progress for resume functionality
            Console.WriteLine($"[DatabaseService] ✅ Tables created");

            // Clear app data only
            Console.WriteLine($"[DatabaseService] Clearing existing data...");
            conn.DeleteAll<DatabaseInfo>();
            conn.DeleteAll<Translation>();
            conn.DeleteAll<AppString>();
            conn.DeleteAll<LanguageOption>();
            conn.DeleteAll<Question>();
            conn.DeleteAll<GiftDescription>();
            conn.DeleteAll<Reflection>();
            conn.DeleteAll<Verse>();
            Console.WriteLine($"[DatabaseService] ✅ Existing data cleared");

            Console.WriteLine($"[DatabaseService] Inserting DatabaseInfo - Version: {rootModel.Database?.Version ?? -1}");
            conn.Insert(rootModel.Database);

            Console.WriteLine($"[DatabaseService] Processing {rootModel.Translations?.Count ?? 0} translations...");
            int translationCount = 0;
            int totalAppStrings = 0;
            int totalLanguageOptions = 0;
            int totalQuestions = 0;
            int totalGiftDescriptions = 0;
            int totalReflections = 0;
            int totalVerses = 0;

        foreach (var translation in rootModel.Translations!)
        {
            translationCount++;
            Console.WriteLine($"[DatabaseService] Processing translation #{translationCount}: {translation.Code}");
            
            try
            {
                conn.Insert(translation);

                Console.WriteLine($"[DatabaseService]   - AppStrings: {translation.AppStrings?.Count ?? 0}");
                foreach (var appString in translation.AppStrings ?? new List<AppString>())
                {
                    appString.TranslationGuid = translation.TranslationGuid;
                    conn.Insert(appString);
                    totalAppStrings++;
                }

                Console.WriteLine($"[DatabaseService]   - LanguageOptions: {translation.LanguageOptions?.Count ?? 0}");
                foreach (var languageOption in translation.LanguageOptions ?? new List<LanguageOption>())
                {
                    languageOption.TranslationGuid = translation.TranslationGuid;
                    conn.Insert(languageOption);
                    totalLanguageOptions++;
                }

                Console.WriteLine($"[DatabaseService]   - Questions: {translation.Questions?.Count ?? 0}");
                foreach (var question in translation.Questions ?? new List<Question>())
                {
                    question.TranslationGuid = translation.TranslationGuid;

                    var match = translation.GiftDescriptions?.FirstOrDefault(gd => gd.Gift == question.Gift);
                    if (match != null)
                    {
                        question.GiftDescriptionGuid = match.GiftDescriptionGuid;
                    }

                    conn.Insert(question);
                    totalQuestions++;
                }

                Console.WriteLine($"[DatabaseService]   - Reflections: {translation.Reflections?.Count ?? 0}");
                foreach (var reflection in translation.Reflections ?? new List<Reflection>())
                {
                    reflection.TranslationGuid = translation.TranslationGuid;
                    conn.Insert(reflection);
                    totalReflections++;
                }

                Console.WriteLine($"[DatabaseService]   - GiftDescriptions: {translation.GiftDescriptions?.Count ?? 0}");
                foreach (var giftDescription in translation.GiftDescriptions ?? new List<GiftDescription>())
                {
                    giftDescription.TranslationGuid = translation.TranslationGuid;
                    conn.Insert(giftDescription);
                    totalGiftDescriptions++;

                    if (giftDescription.Verses != null)
                    {
                        foreach (var verse in giftDescription.Verses)
                        {
                            verse.GiftDescriptionGuid = giftDescription.GiftDescriptionGuid;
                            conn.Insert(verse);
                            totalVerses++;
                        }
                    }
                }
                
                Console.WriteLine($"[DatabaseService] ✅ Translation '{translation.Code}' processed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] ❌ Error processing translation '{translation.Code}': {ex.Message}");
                Console.WriteLine($"[DatabaseService] Exception Type: {ex.GetType().Name}");
                Console.WriteLine($"[DatabaseService] Stack Trace: {ex.StackTrace}");
                throw; // Re-throw to handle at higher level
            }
        }

        Console.WriteLine($"[DatabaseService] ========================================");
        Console.WriteLine($"[DatabaseService] Database Refresh Summary:");
        Console.WriteLine($"[DatabaseService]   Translations: {translationCount}");
        Console.WriteLine($"[DatabaseService]   AppStrings: {totalAppStrings}");
        Console.WriteLine($"[DatabaseService]   LanguageOptions: {totalLanguageOptions}");
        Console.WriteLine($"[DatabaseService]   Questions: {totalQuestions}");
        Console.WriteLine($"[DatabaseService]   GiftDescriptions: {totalGiftDescriptions}");
        Console.WriteLine($"[DatabaseService]   Reflections: {totalReflections}");
        Console.WriteLine($"[DatabaseService]   Verses: {totalVerses}");
        Console.WriteLine($"[DatabaseService] ========================================");

        Console.WriteLine($"[DatabaseService] Sending LanguageChangedMessage...");
        WeakReferenceMessenger.Default.Send(new LanguageChangedMessage(true));
        
        Console.WriteLine($"[DatabaseService] ✅ RefreshDatabaseAsync completed successfully");
        return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DatabaseService] ❌ CRITICAL ERROR in RefreshDatabaseAsync");
            Console.WriteLine($"[DatabaseService] Exception: {ex.Message}");
            Console.WriteLine($"[DatabaseService] Exception Type: {ex.GetType().Name}");
            Console.WriteLine($"[DatabaseService] Stack Trace: {ex.StackTrace}");
            Console.WriteLine($"[DatabaseService] ========================================");
            return false;
        }
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

        var translation = conn.Table<Translation>().FirstOrDefault(t => t.Code.ToLower() == languageCode.ToLower());
        if (translation == null)
            return new List<Reflection>();

        return conn.Table<Reflection>()
                   .Where(r => r.TranslationGuid == translation.TranslationGuid)
                   .OrderBy(r => r.Number)
                   .ToList();
    }

    private async Task<bool> EnsureDatabaseUpToDate()
    {
        Console.WriteLine($"[DatabaseService] ========================================");
        Console.WriteLine($"[DatabaseService] Starting EnsureDatabaseUpToDate");
        Console.WriteLine($"[DatabaseService] Database Path: {DatabasePath}");
        
        // 1️. If the DB file does not exist, fetch fresh data
        bool fileExists = File.Exists(DatabasePath);
        Console.WriteLine($"[DatabaseService] Database file exists: {fileExists}");
        
        if (!fileExists)
        {
            Console.WriteLine($"[DatabaseService] Database file missing - triggering RefreshDatabaseAsync");
            bool result = await RefreshDatabaseAsync();
            Console.WriteLine($"[DatabaseService] RefreshDatabaseAsync result: {result}");
            Console.WriteLine($"[DatabaseService] ========================================");
            return result;
        }

        try
        {
            Console.WriteLine($"[DatabaseService] Opening SQLite connection for version check...");
            using var conn = new SQLiteConnection(DatabasePath);
            Console.WriteLine($"[DatabaseService] ✅ Connection opened");

            // 2️. If the DatabaseInfo table does not exist, fetch fresh data
            Console.WriteLine($"[DatabaseService] Checking if DatabaseInfo table exists...");
            var tableInfo = conn.GetTableInfo(nameof(DatabaseInfo));
            Console.WriteLine($"[DatabaseService] DatabaseInfo table info: {tableInfo?.Count ?? 0} columns");
            
            if (tableInfo == null || tableInfo.Count == 0)
            {
                Console.WriteLine($"[DatabaseService] DatabaseInfo table missing - triggering RefreshDatabaseAsync");
                bool result = await RefreshDatabaseAsync();
                Console.WriteLine($"[DatabaseService] RefreshDatabaseAsync result: {result}");
                Console.WriteLine($"[DatabaseService] ========================================");
                return result;
            }

            // 3. Get local version
            Console.WriteLine($"[DatabaseService] Fetching local database version...");
            var localInfo = conn.Table<DatabaseInfo>().FirstOrDefault();
            
            if (localInfo != null)
            {
                Console.WriteLine($"[DatabaseService] Local database version: {localInfo.Version}");
            }
            else
            {
                Console.WriteLine($"[DatabaseService] No local DatabaseInfo record found");
            }

            // 4️. Get remote version only
            Console.WriteLine($"[DatabaseService] Fetching remote database version...");
            var remoteVersionResult = await _urlService.GetRemoteDatabaseVersionAsync();
            
            Console.WriteLine($"[DatabaseService] Remote version fetch result - IsSuccess: {remoteVersionResult.IsSuccess}");
            
            if (!remoteVersionResult.IsSuccess)
            {
                if (remoteVersionResult.Error != null)
                {
                    Console.WriteLine($"[DatabaseService] ❌ Failed to get remote version");
                    Console.WriteLine($"[DatabaseService] Error: {remoteVersionResult.Error.ExceptionTitle} - {remoteVersionResult.Error.Message}");
                }
                Console.WriteLine($"[DatabaseService] ========================================");
                return false;
            }

            var remoteVersion = remoteVersionResult.Value;
            Console.WriteLine($"[DatabaseService] Remote database version: {remoteVersion}");

            if (localInfo == null || remoteVersion > localInfo.Version)
            {
                Console.WriteLine($"[DatabaseService] Database update needed (local: {localInfo?.Version ?? -1}, remote: {remoteVersion})");
                Console.WriteLine($"[DatabaseService] Triggering RefreshDatabaseAsync...");
                bool result = await RefreshDatabaseAsync();
                Console.WriteLine($"[DatabaseService] RefreshDatabaseAsync result: {result}");
                Console.WriteLine($"[DatabaseService] ========================================");
                return result;
            }

            Console.WriteLine($"[DatabaseService] ✅ Database is up-to-date (version {localInfo.Version})");
            Console.WriteLine($"[DatabaseService] ========================================");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DatabaseService] ❌ CRITICAL ERROR in EnsureDatabaseUpToDate");
            Console.WriteLine($"[DatabaseService] Exception: {ex.Message}");
            Console.WriteLine($"[DatabaseService] Exception Type: {ex.GetType().Name}");
            Console.WriteLine($"[DatabaseService] Stack Trace: {ex.StackTrace}");
            Console.WriteLine($"[DatabaseService] ========================================");
            return false;
        }
    }
    private SQLiteAsyncConnection GetAsyncConnection()
    {
        return new SQLiteAsyncConnection(DatabasePath);
    }
}
