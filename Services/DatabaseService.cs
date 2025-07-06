using SpiritualGiftsTest.Models;
using SQLite;

namespace SpiritualGiftsTest.Services;

public interface IDatabaseService
{
    Task<Translation?> GetTranslationByCodeAsync(string languageCode);
    Task<IEnumerable<LanguageOption>> GetLanguageOptionsAsync(string languageCode);
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
        return conn.Table<Translation>().FirstOrDefault(t => t.Code == languageCode);
    }

    public async Task<IEnumerable<LanguageOption>> GetLanguageOptionsAsync(string languageCode)
    {
        var translation = await GetTranslationByCodeAsync(languageCode);
        return translation?.LanguageOptions ?? new List<LanguageOption>();
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

        conn.Insert(rootModel.Database);

        foreach (var translation in rootModel.Translations)
        {
            conn.Insert(translation);
            conn.InsertAll(translation.AppStrings);
            conn.InsertAll(translation.LanguageOptions);
            conn.InsertAll(translation.Questions);
            conn.InsertAll(translation.GiftDescriptions);
            conn.InsertAll(translation.Reflections);
        }

        return true;
    }
}
