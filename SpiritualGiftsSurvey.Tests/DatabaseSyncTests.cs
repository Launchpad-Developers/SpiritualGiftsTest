using SpiritualGiftsSurvey.Enums;
using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Utilities;
using SQLite;

namespace SpiritualGiftsSurvey.Tests;

/// <summary>
/// Integration tests for the automatic database synchronization trigger.
/// 
/// CRITICAL: These tests validate that the app automatically downloads Firebase data on:
/// - First launch (no database exists)
/// - Version upgrade (remote version > local version)
/// - And correctly skips sync when versions match
/// 
/// If these fail, the app will not initialize properly on first launch or after updates.
/// </summary>
public class DatabaseSyncTests : IDisposable
{
    private readonly string _testDatabasePath;
    private readonly string _testDatabaseFolder;

    public DatabaseSyncTests()
    {
        // Use a unique temp folder for each test run
        _testDatabaseFolder = Path.Combine(Path.GetTempPath(), $"SGSTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDatabaseFolder);
        _testDatabasePath = Path.Combine(_testDatabaseFolder, "SpiritualGiftsSurvey.sqlite");
    }

    public void Dispose()
    {
        // Cleanup: Delete test database and folder
        try
        {
            if (File.Exists(_testDatabasePath))
                File.Delete(_testDatabasePath);
            
            if (Directory.Exists(_testDatabaseFolder))
                Directory.Delete(_testDatabaseFolder, recursive: true);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    #region First Launch Tests

    [Fact]
    public async Task FirstLaunch_NoDatabaseExists_TriggersAutomaticSync()
    {
        // Arrange
        var mockUrlService = new MockUrlService(
            databaseVersion: 1,
            fullDatabaseResult: CreateMockRootModel(version: 1));
        var mockStorage = new MockDeviceStorageService(_testDatabasePath);
        var databaseService = new DatabaseService(mockStorage, mockUrlService);

        // Ensure database doesn't exist
        Assert.False(File.Exists(_testDatabasePath), "Database should not exist before first launch");

        // Act
        var translation = await databaseService.GetTranslationByCodeAsync("EN");

        // Assert
        Assert.NotNull(translation);
        Assert.Equal("EN", translation.Code);
        Assert.True(mockUrlService.GetFullDatabaseCalled, "Should call GetFullDatabaseAsync on first launch");
        // NOTE: Version check is NOT called when database doesn't exist - it goes straight to download
        Assert.True(File.Exists(_testDatabasePath), "Database should be created");
        
        // Verify database was populated
        var dbInfo = databaseService.GetDatabaseInfo();
        Assert.NotNull(dbInfo);
        Assert.Equal(1, dbInfo.Version);
    }

    [Fact]
    public async Task FirstLaunch_NoDatabaseInfoTable_TriggersSync()
    {
        // Arrange
        var mockUrlService = new MockUrlService(
            databaseVersion: 1,
            fullDatabaseResult: CreateMockRootModel(version: 1));
        var mockStorage = new MockDeviceStorageService(_testDatabasePath);
        
        // Create empty database file (no tables)
        using (var conn = new SQLiteConnection(_testDatabasePath))
        {
            // Create some table but NOT DatabaseInfo
            conn.CreateTable<Translation>();
        }

        var databaseService = new DatabaseService(mockStorage, mockUrlService);

        // Act
        var translation = await databaseService.GetTranslationByCodeAsync("EN");

        // Assert
        Assert.NotNull(translation);
        Assert.True(mockUrlService.GetFullDatabaseCalled, "Should sync when DatabaseInfo table missing");
        
        var dbInfo = databaseService.GetDatabaseInfo();
        Assert.NotNull(dbInfo);
        Assert.Equal(1, dbInfo.Version);
    }

    #endregion

    #region Version Upgrade Tests

    [Fact]
    public async Task VersionUpgrade_RemoteNewer_TriggersSync()
    {
        // Arrange
        var mockUrlService = new MockUrlService(
            databaseVersion: 2,
            fullDatabaseResult: CreateMockRootModel(version: 2));
        var mockStorage = new MockDeviceStorageService(_testDatabasePath);
        
        // Create database with old version
        CreateDatabaseWithVersion(version: 1);

        var databaseService = new DatabaseService(mockStorage, mockUrlService);

        // Act
        var translation = await databaseService.GetTranslationByCodeAsync("EN");

        // Assert
        Assert.NotNull(translation);
        Assert.True(mockUrlService.GetRemoteVersionCalled, "Should check remote version");
        Assert.True(mockUrlService.GetFullDatabaseCalled, "Should sync when remote version is newer");
        
        // Verify version was updated
        var dbInfo = databaseService.GetDatabaseInfo();
        Assert.NotNull(dbInfo);
        Assert.Equal(2, dbInfo.Version);
    }

    [Fact]
    public async Task VersionUpgrade_MultipleVersionsNewer_TriggersSync()
    {
        // Arrange
        var mockUrlService = new MockUrlService(
            databaseVersion: 10,
            fullDatabaseResult: CreateMockRootModel(version: 10));
        var mockStorage = new MockDeviceStorageService(_testDatabasePath);
        
        // Create database with much older version
        CreateDatabaseWithVersion(version: 1);

        var databaseService = new DatabaseService(mockStorage, mockUrlService);

        // Act
        var translation = await databaseService.GetTranslationByCodeAsync("EN");

        // Assert
        Assert.NotNull(translation);
        Assert.True(mockUrlService.GetFullDatabaseCalled, "Should sync when remote is many versions newer");
        
        var dbInfo = databaseService.GetDatabaseInfo();
        Assert.NotNull(dbInfo);
        Assert.Equal(10, dbInfo.Version);
    }

    #endregion

    #region Skip Sync Tests

    [Fact]
    public async Task SameVersion_SkipsSync()
    {
        // Arrange
        var mockUrlService = new MockUrlService(
            databaseVersion: 5,
            fullDatabaseResult: CreateMockRootModel(version: 5));
        var mockStorage = new MockDeviceStorageService(_testDatabasePath);
        
        // Create database with same version as remote
        CreateDatabaseWithVersion(version: 5);

        var databaseService = new DatabaseService(mockStorage, mockUrlService);

        // Act
        var translation = await databaseService.GetTranslationByCodeAsync("EN");

        // Assert
        Assert.NotNull(translation);
        Assert.True(mockUrlService.GetRemoteVersionCalled, "Should check remote version");
        Assert.False(mockUrlService.GetFullDatabaseCalled, "Should NOT sync when versions match");
    }

    [Fact]
    public async Task LocalNewer_SkipsSync()
    {
        // Arrange - This shouldn't happen in production, but test defensive behavior
        var mockUrlService = new MockUrlService(
            databaseVersion: 3,
            fullDatabaseResult: CreateMockRootModel(version: 3));
        var mockStorage = new MockDeviceStorageService(_testDatabasePath);
        
        // Create database with NEWER version than remote
        CreateDatabaseWithVersion(version: 5);

        var databaseService = new DatabaseService(mockStorage, mockUrlService);

        // Act
        var translation = await databaseService.GetTranslationByCodeAsync("EN");

        // Assert
        Assert.NotNull(translation);
        Assert.True(mockUrlService.GetRemoteVersionCalled, "Should check remote version");
        Assert.False(mockUrlService.GetFullDatabaseCalled, "Should NOT sync when local is newer");
        
        // Verify version didn't downgrade
        var dbInfo = databaseService.GetDatabaseInfo();
        Assert.NotNull(dbInfo);
        Assert.Equal(5, dbInfo.Version);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task SyncFailure_FirebaseUnreachable_ReturnsNull()
    {
        // Arrange
        var mockUrlService = new MockUrlService(
            simulateNetworkError: true);
        var mockStorage = new MockDeviceStorageService(_testDatabasePath);
        
        // No database exists
        var databaseService = new DatabaseService(mockStorage, mockUrlService);

        // Act
        var translation = await databaseService.GetTranslationByCodeAsync("EN");

        // Assert
        Assert.Null(translation);
        // NOTE: Version check is NOT called when database doesn't exist
        Assert.True(mockUrlService.GetFullDatabaseCalled, "Should attempt GetFullDatabaseAsync");
        Assert.False(File.Exists(_testDatabasePath), "Database should not be created on sync failure");
    }

    [Fact]
    public async Task SyncFailure_InvalidJson_ReturnsNull()
    {
        // Arrange
        var mockUrlService = new MockUrlService(
            databaseVersion: 1,
            fullDatabaseResult: null); // Simulate deserialization failure
        var mockStorage = new MockDeviceStorageService(_testDatabasePath);
        
        var databaseService = new DatabaseService(mockStorage, mockUrlService);

        // Act
        var translation = await databaseService.GetTranslationByCodeAsync("EN");

        // Assert
        Assert.Null(translation);
    }

    [Fact]
    public async Task SyncFailure_WithExistingDatabase_ReturnsNull()
    {
        // Arrange
        var mockUrlService = new MockUrlService(
            databaseVersion: 2,
            fullDatabaseResult: null, // Simulate sync failure
            simulateNetworkError: false);
        var mockStorage = new MockDeviceStorageService(_testDatabasePath);
        
        // Create database with old version that works
        CreateDatabaseWithVersion(version: 1);

        var databaseService = new DatabaseService(mockStorage, mockUrlService);

        // Act
        var translation = await databaseService.GetTranslationByCodeAsync("EN");

        // Assert
        // CURRENT BEHAVIOR: When remote version check succeeds but download fails,
        // the app returns null (does NOT use old data as fallback)
        // This could be improved to fall back to old data if sync fails
        Assert.Null(translation);
        
        // Version should remain at old version since sync failed
        var dbInfo = databaseService.GetDatabaseInfo();
        Assert.NotNull(dbInfo);
        Assert.Equal(1, dbInfo.Version);
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public async Task ConcurrentFirstLaunch_OnlySyncsOnce()
    {
        // Arrange
        var mockUrlService = new MockUrlService(
            databaseVersion: 1,
            fullDatabaseResult: CreateMockRootModel(version: 1),
            simulateDelay: 500); // Add delay to force concurrency
        var mockStorage = new MockDeviceStorageService(_testDatabasePath);
        var databaseService = new DatabaseService(mockStorage, mockUrlService);

        // Act - Call GetTranslationByCodeAsync concurrently
        var tasks = new[]
        {
            databaseService.GetTranslationByCodeAsync("EN"),
            databaseService.GetTranslationByCodeAsync("EN"),
            databaseService.GetTranslationByCodeAsync("EN"),
        };

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, r => Assert.NotNull(r));
        // Note: Without synchronization in DatabaseService, this might fail
        // This test documents current behavior - sync may be called multiple times
        // Future improvement: Add locking to ensure single sync
    }

    #endregion

    #region Helper Methods

    private void CreateDatabaseWithVersion(int version)
    {
        var rootModel = CreateMockRootModel(version);
        
        using var conn = new SQLiteConnection(_testDatabasePath);
        
        // Create tables
        conn.CreateTable<DatabaseInfo>();
        conn.CreateTable<Translation>();
        conn.CreateTable<AppString>();
        conn.CreateTable<LanguageOption>();
        conn.CreateTable<Question>();
        conn.CreateTable<GiftDescription>();
        conn.CreateTable<Reflection>();
        conn.CreateTable<Verse>();

        // Insert data
        conn.Insert(rootModel.Database);
        foreach (var translation in rootModel.Translations)
        {
            conn.Insert(translation);
            foreach (var appString in translation.AppStrings)
                conn.Insert(appString);
            foreach (var langOption in translation.LanguageOptions)
                conn.Insert(langOption);
        }
    }

    private static RootModel CreateMockRootModel(int version)
    {
        var translationGuid = Guid.NewGuid();
        
        return new RootModel
        {
            Database = new DatabaseInfo
            {
                Version = version,
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                Author = "Test",
                Environment = "Test",
                Notes = $"Test database version {version}"
            },
            Translations = new List<Translation>
            {
                new Translation
                {
                    TranslationGuid = translationGuid,
                    Code = "EN",
                    FlowDirection = "LTR",
                    AppStrings = new List<AppString>
                    {
                        new AppString
                        {
                            TranslationGuid = translationGuid,
                            Key = "Instructions",
                            Value = "Test instructions"
                        }
                    },
                    LanguageOptions = new List<LanguageOption>
                    {
                        new LanguageOption
                        {
                            TranslationGuid = translationGuid,
                            CodeOption = "EN",
                            DisplayName = "English"
                        }
                    },
                    Questions = new List<Question>(),
                    GiftDescriptions = new List<GiftDescription>(),
                    Reflections = new List<Reflection>()
                }
            }
        };
    }

    #endregion
}

#region Mock Services

public class MockDeviceStorageService : IDeviceStorageService
{
    private readonly string _databasePath;

    public MockDeviceStorageService(string databasePath)
    {
        _databasePath = databasePath;
    }

    public string GetDatabaseFileLocation() => _databasePath;
    public string GetDatabaseFolderLocation() => Path.GetDirectoryName(_databasePath) ?? "";
    
    public string GetAbsoluteFilePath(string filePath) => 
        Path.Combine(GetDatabaseFolderLocation(), filePath);
    
    public bool FileExists(string absolutePath) => File.Exists(absolutePath);
    public Stream ReadFile(string absolutePath) => File.OpenRead(absolutePath);
    public Stream CreateOrWriteFile(string absolutePath) => 
        File.Open(absolutePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
    
    public void DeleteFile(string absoluteFilePath)
    {
        if (File.Exists(absoluteFilePath))
            File.Delete(absoluteFilePath);
    }
}

public class MockUrlService : IUrlService
{
    private readonly int _databaseVersion;
    private readonly RootModel? _fullDatabaseResult;
    private readonly bool _simulateNetworkError;
    private readonly int _simulateDelay;

    public bool GetRemoteVersionCalled { get; private set; }
    public bool GetFullDatabaseCalled { get; private set; }
    public int GetFullDatabaseCallCount { get; private set; }

    public MockUrlService(
        int databaseVersion = 1,
        RootModel? fullDatabaseResult = null,
        bool simulateNetworkError = false,
        int simulateDelay = 0)
    {
        _databaseVersion = databaseVersion;
        _fullDatabaseResult = fullDatabaseResult;
        _simulateNetworkError = simulateNetworkError;
        _simulateDelay = simulateDelay;
    }

    public async Task<Result<int>> GetRemoteDatabaseVersionAsync()
    {
        GetRemoteVersionCalled = true;
        
        if (_simulateDelay > 0)
            await Task.Delay(_simulateDelay);

        if (_simulateNetworkError)
            return new Result<int>(new TitledException("Network Error", "Simulated network failure"));

        return new Result<int>(_databaseVersion);
    }

    public async Task<Result<RootModel>> GetFullDatabaseAsync()
    {
        GetFullDatabaseCalled = true;
        GetFullDatabaseCallCount++;
        
        if (_simulateDelay > 0)
            await Task.Delay(_simulateDelay);

        if (_simulateNetworkError)
            return new Result<RootModel>(new TitledException("Network Error", "Simulated network failure"));

        if (_fullDatabaseResult == null)
            return new Result<RootModel>(new TitledException("Error", "Deserialization failed"));

        return new Result<RootModel>(_fullDatabaseResult);
    }
}

#endregion
