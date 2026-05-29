using SpiritualGiftsSurvey.Enums;
using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Utilities;
using System.Text.Json;

namespace SpiritualGiftsSurvey.Tests;

/// <summary>
/// Tests for SurveyProgressService - survey state persistence and restoration
/// </summary>
public class SurveyProgressTests : IDisposable
{
    private readonly string _testDbFolder;
    private readonly TestDeviceStorageService _mockStorage;
    private readonly SurveyProgressService _progressService;

    public SurveyProgressTests()
    {
        _testDbFolder = Path.Combine(Path.GetTempPath(), $"SurveyProgressTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDbFolder);
        
        var databasePath = Path.Combine(_testDbFolder, "SpiritualGiftsSurvey.sqlite");
        
        _mockStorage = new TestDeviceStorageService(databasePath);
        _progressService = new SurveyProgressService(_mockStorage);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDbFolder))
        {
            Directory.Delete(_testDbFolder, true);
        }
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task SaveProgressAsync_CreatesNewProgress()
    {
        // Arrange
        var progress = new SurveyProgress
        {
            SessionGuid = Guid.NewGuid(),
            StartedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            LanguageCode = "en",
            CurrentPage = 1,
            QuestionOrderJson = JsonSerializer.Serialize(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }),
            AnswersJson = JsonSerializer.Serialize(new Dictionary<Guid, UserValue>())
        };

        // Act
        await _progressService.SaveProgressAsync(progress);
        var retrieved = await _progressService.GetActiveProgressAsync();

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(progress.SessionGuid, retrieved.SessionGuid);
        Assert.Equal(progress.LanguageCode, retrieved.LanguageCode);
        Assert.Equal(progress.CurrentPage, retrieved.CurrentPage);
    }

    [Fact]
    public async Task SaveProgressAsync_EnforcesSingleActiveProgress()
    {
        // Arrange
        var progress1 = new SurveyProgress
        {
            SessionGuid = Guid.NewGuid(),
            StartedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            LanguageCode = "en",
            CurrentPage = 1,
            QuestionOrderJson = JsonSerializer.Serialize(new List<Guid> { Guid.NewGuid() }),
            AnswersJson = JsonSerializer.Serialize(new Dictionary<Guid, UserValue>())
        };

        var progress2 = new SurveyProgress
        {
            SessionGuid = Guid.NewGuid(),
            StartedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            LanguageCode = "en",
            CurrentPage = 2,
            QuestionOrderJson = JsonSerializer.Serialize(new List<Guid> { Guid.NewGuid() }),
            AnswersJson = JsonSerializer.Serialize(new Dictionary<Guid, UserValue>())
        };

        // Act
        await _progressService.SaveProgressAsync(progress1);
        await _progressService.SaveProgressAsync(progress2);
        var retrieved = await _progressService.GetActiveProgressAsync();

        // Assert - Only the second progress should exist
        Assert.NotNull(retrieved);
        Assert.Equal(progress2.SessionGuid, retrieved.SessionGuid);
        Assert.Equal(2, retrieved.CurrentPage);
    }

    [Fact]
    public async Task GetActiveProgressAsync_ReturnsNullWhenNoProgress()
    {
        // Act
        var retrieved = await _progressService.GetActiveProgressAsync();

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task UpdateAnswerAsync_UpdatesExistingAnswer()
    {
        // Arrange
        var questionGuid = Guid.NewGuid();
        var answers = new Dictionary<Guid, UserValue> { { questionGuid, UserValue.NotAtAll } };
        
        var progress = new SurveyProgress
        {
            SessionGuid = Guid.NewGuid(),
            StartedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            LanguageCode = "en",
            CurrentPage = 1,
            QuestionOrderJson = JsonSerializer.Serialize(new List<Guid> { questionGuid }),
            AnswersJson = JsonSerializer.Serialize(answers)
        };

        await _progressService.SaveProgressAsync(progress);

        // Act
        await _progressService.UpdateAnswerAsync(questionGuid, UserValue.Much);
        var retrieved = await _progressService.GetActiveProgressAsync();

        // Assert
        Assert.NotNull(retrieved);
        var retrievedAnswers = JsonSerializer.Deserialize<Dictionary<Guid, UserValue>>(retrieved.AnswersJson);
        Assert.NotNull(retrievedAnswers);
        Assert.Equal(UserValue.Much, retrievedAnswers[questionGuid]);
    }

    [Fact]
    public async Task UpdateAnswerAsync_AddsNewAnswer()
    {
        // Arrange
        var existingGuid = Guid.NewGuid();
        var newGuid = Guid.NewGuid();
        var answers = new Dictionary<Guid, UserValue> { { existingGuid, UserValue.Some } };
        
        var progress = new SurveyProgress
        {
            SessionGuid = Guid.NewGuid(),
            StartedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            LanguageCode = "en",
            CurrentPage = 1,
            QuestionOrderJson = JsonSerializer.Serialize(new List<Guid> { existingGuid, newGuid }),
            AnswersJson = JsonSerializer.Serialize(answers)
        };

        await _progressService.SaveProgressAsync(progress);

        // Act
        await _progressService.UpdateAnswerAsync(newGuid, UserValue.Little);
        var retrieved = await _progressService.GetActiveProgressAsync();

        // Assert
        Assert.NotNull(retrieved);
        var retrievedAnswers = JsonSerializer.Deserialize<Dictionary<Guid, UserValue>>(retrieved.AnswersJson);
        Assert.NotNull(retrievedAnswers);
        Assert.Equal(2, retrievedAnswers.Count);
        Assert.Equal(UserValue.Some, retrievedAnswers[existingGuid]);
        Assert.Equal(UserValue.Little, retrievedAnswers[newGuid]);
    }

    [Fact]
    public async Task UpdateCurrentPageAsync_UpdatesPage()
    {
        // Arrange
        var progress = new SurveyProgress
        {
            SessionGuid = Guid.NewGuid(),
            StartedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            LanguageCode = "en",
            CurrentPage = 1,
            QuestionOrderJson = JsonSerializer.Serialize(new List<Guid> { Guid.NewGuid() }),
            AnswersJson = JsonSerializer.Serialize(new Dictionary<Guid, UserValue>())
        };

        await _progressService.SaveProgressAsync(progress);

        // Act
        await _progressService.UpdateCurrentPageAsync(5);
        var retrieved = await _progressService.GetActiveProgressAsync();

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(5, retrieved.CurrentPage);
    }

    [Fact]
    public async Task ClearProgressAsync_RemovesAllProgress()
    {
        // Arrange
        var progress = new SurveyProgress
        {
            SessionGuid = Guid.NewGuid(),
            StartedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            LanguageCode = "en",
            CurrentPage = 1,
            QuestionOrderJson = JsonSerializer.Serialize(new List<Guid> { Guid.NewGuid() }),
            AnswersJson = JsonSerializer.Serialize(new Dictionary<Guid, UserValue>())
        };

        await _progressService.SaveProgressAsync(progress);

        // Act
        await _progressService.ClearProgressAsync();
        var retrieved = await _progressService.GetActiveProgressAsync();

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task HasActiveProgressAsync_ReturnsTrueWhenProgressExists()
    {
        // Arrange
        var progress = new SurveyProgress
        {
            SessionGuid = Guid.NewGuid(),
            StartedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            LanguageCode = "en",
            CurrentPage = 1,
            QuestionOrderJson = JsonSerializer.Serialize(new List<Guid> { Guid.NewGuid() }),
            AnswersJson = JsonSerializer.Serialize(new Dictionary<Guid, UserValue>())
        };

        await _progressService.SaveProgressAsync(progress);

        // Act
        var hasProgress = await _progressService.HasActiveProgressAsync();

        // Assert
        Assert.True(hasProgress);
    }

    [Fact]
    public async Task HasActiveProgressAsync_ReturnsFalseWhenNoProgress()
    {
        // Act
        var hasProgress = await _progressService.HasActiveProgressAsync();

        // Assert
        Assert.False(hasProgress);
    }

    [Fact]
    public async Task ProgressPersistsAcrossServiceInstances()
    {
        // Arrange
        var sessionGuid = Guid.NewGuid();
        var progress = new SurveyProgress
        {
            SessionGuid = sessionGuid,
            StartedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            LanguageCode = "en",
            CurrentPage = 3,
            QuestionOrderJson = JsonSerializer.Serialize(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }),
            AnswersJson = JsonSerializer.Serialize(new Dictionary<Guid, UserValue> 
            { 
                { Guid.NewGuid(), UserValue.Much } 
            })
        };

        await _progressService.SaveProgressAsync(progress);

        // Act - Create new service instance pointing to same storage
        var newProgressService = new SurveyProgressService(_mockStorage);
        var retrieved = await newProgressService.GetActiveProgressAsync();

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(sessionGuid, retrieved.SessionGuid);
        Assert.Equal(3, retrieved.CurrentPage);
    }

    [Fact]
    public async Task UpdateAnswerAsync_HandlesEmptyInitialAnswers()
    {
        // Arrange
        var questionGuid = Guid.NewGuid();
        var progress = new SurveyProgress
        {
            SessionGuid = Guid.NewGuid(),
            StartedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            LanguageCode = "en",
            CurrentPage = 1,
            QuestionOrderJson = JsonSerializer.Serialize(new List<Guid> { questionGuid }),
            AnswersJson = JsonSerializer.Serialize(new Dictionary<Guid, UserValue>())
        };

        await _progressService.SaveProgressAsync(progress);

        // Act
        await _progressService.UpdateAnswerAsync(questionGuid, UserValue.Much);
        var retrieved = await _progressService.GetActiveProgressAsync();

        // Assert
        Assert.NotNull(retrieved);
        var retrievedAnswers = JsonSerializer.Deserialize<Dictionary<Guid, UserValue>>(retrieved.AnswersJson);
        Assert.NotNull(retrievedAnswers);
        Assert.Single(retrievedAnswers);
        Assert.Equal(UserValue.Much, retrievedAnswers[questionGuid]);
    }

    private class TestDeviceStorageService : IDeviceStorageService
    {
        private readonly string _databasePath;
        public TestDeviceStorageService(string databasePath) => _databasePath = databasePath;
        public string GetDatabaseFileLocation() => _databasePath;
        public string GetDatabaseFolderLocation() => Path.GetDirectoryName(_databasePath) ?? "";
        public string GetAbsoluteFilePath(string filePath) => filePath;
        public bool FileExists(string absolutePath) => File.Exists(absolutePath);
        public Stream ReadFile(string absolutePath) => File.OpenRead(absolutePath);
        public Stream CreateOrWriteFile(string absolutePath) => File.Create(absolutePath);
        public void DeleteFile(string absoluteFilePath) => File.Delete(absoluteFilePath);
    }
}
