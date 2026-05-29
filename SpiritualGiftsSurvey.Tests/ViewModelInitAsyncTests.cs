using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Utilities;
using Xunit;

namespace SpiritualGiftsSurvey.Tests;

/// <summary>
/// Tests for ViewModel error handling patterns
/// These tests verify defensive programming patterns that prevent infinite loading spinners
/// and ensure graceful fallback when services fail
/// </summary>
public class ViewModelErrorPatternTests
{
    [Fact]
    public async Task DatabaseService_GetQuestionsAsync_NoDatabase_ReturnsEmptyList()
    {
        // Arrange
        var deviceStorage = new TestDeviceStorageService2();
        var urlService = new TestUrlService2();
        var dbService = new DatabaseService(deviceStorage, urlService);
        
        // Ensure database doesn't exist
        if (File.Exists(deviceStorage.GetDatabaseFileLocation()))
            File.Delete(deviceStorage.GetDatabaseFileLocation());
        
        // Act
        var questions = await dbService.GetQuestionsAsync("en");
        
        // Assert - Should return empty list, NOT throw exception
        Assert.NotNull(questions);
        Assert.Empty(questions);
    }
    
    [Fact]
    public async Task DatabaseService_GetGiftDescriptionsAsync_NoDatabase_ReturnsEmptyList()
    {
        // Arrange
        var deviceStorage = new TestDeviceStorageService2();
        var urlService = new TestUrlService2();
        var dbService = new DatabaseService(deviceStorage, urlService);
        
        // Ensure database doesn't exist
        if (File.Exists(deviceStorage.GetDatabaseFileLocation()))
            File.Delete(deviceStorage.GetDatabaseFileLocation());
        
        // Act
        var descriptions = await dbService.GetGiftDescriptionsAsync("en");
        
        // Assert - Should return empty list, NOT throw exception
        Assert.NotNull(descriptions);
        Assert.Empty(descriptions);
    }
    
    [Fact]
    public async Task DatabaseService_RefreshDatabaseAsync_NetworkFailure_ReturnsFalse()
    {
        // Arrange
        var deviceStorage = new TestDeviceStorageService2();
        var urlService = new TestUrlService2();
        urlService.SetupToFailNetworkCalls();
        var dbService = new DatabaseService(deviceStorage, urlService);
        
        // Act
        var result = await dbService.RefreshDatabaseAsync();
        
        // Assert - Should return false gracefully, NOT throw
        Assert.False(result);
    }
    
    [Fact]
    public async Task DatabaseService_RefreshDatabaseAsync_InvalidJson_ReturnsFalse()
    {
        // Arrange
        var deviceStorage = new TestDeviceStorageService2();
        var urlService = new TestUrlService2();
        urlService.SetupToReturnInvalidJson();
        var dbService = new DatabaseService(deviceStorage, urlService);
        
        // Act
        var result = await dbService.RefreshDatabaseAsync();
        
        // Assert - Should handle gracefully
        Assert.False(result);
    }
}

#region Test Helper Services

public class TestDeviceStorageService2 : IDeviceStorageService
{
    private readonly string _testDbPath;
    private int? _version;
    
    public TestDeviceStorageService2()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_vm_patterns_{Guid.NewGuid()}.sqlite");
    }
    
    public string GetDatabaseFileLocation() => _testDbPath;
    public string GetDatabaseFolderLocation() => Path.GetDirectoryName(_testDbPath) ?? Path.GetTempPath();
    public int? GetLocalDatabaseVersion() => _version;
    public void SetLocalDatabaseVersion(int version) => _version = version;
    
    // File system stubs
    public string GetAbsoluteFilePath(string filePath) => filePath;
    public bool FileExists(string absolutePath) => File.Exists(absolutePath);
    public Stream ReadFile(string absolutePath) => File.OpenRead(absolutePath);
    public Stream CreateOrWriteFile(string absolutePath) => File.Create(absolutePath);
    public void DeleteFile(string absoluteFilePath) => File.Delete(absoluteFilePath);
}

public class TestUrlService2 : IUrlService
{
    private bool _networkFails;
    private bool _returnInvalidJson;
    
    public void SetupToFailNetworkCalls() => _networkFails = true;
    public void SetupToReturnInvalidJson() => _returnInvalidJson = true;
    
    public async Task<string?> GetStringAsync(string url)
    {
        await Task.Yield();
        
        if (_networkFails)
            return null;
            
        if (_returnInvalidJson)
            return "{ invalid json }";
            
        // Return minimal valid RootModel JSON for success case
        return @"{
            ""Database"": { ""Version"": 1 },
            ""Translations"": []
        }";
    }
    
    public async Task<Result<RootModel>> GetFullDatabaseAsync()
    {
        await Task.Yield();
        
        if (_networkFails)
            return new Result<RootModel>(new TitledException("Network error", "Failed to fetch database"));
        
        if (_returnInvalidJson)
            return new Result<RootModel>(new TitledException("Invalid JSON", "Failed to parse database"));
        
        // Return minimal valid model
        var model = new RootModel
        {
            Database = new DatabaseInfo { Version = 1 },
            Translations = new List<Translation>()
        };
        
        return new Result<RootModel>(model);
    }
    
    public async Task<Result<int>> GetRemoteDatabaseVersionAsync()
    {
        await Task.Yield();
        if (_networkFails)
            return new Result<int>(new TitledException("Network error", "Failed to fetch version"));
        
        return new Result<int>(1);
    }
}

#endregion
