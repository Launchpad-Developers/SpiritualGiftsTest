using SpiritualGiftsSurvey.Enums;
using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Utilities;
using SQLite;

namespace SpiritualGiftsSurvey.Tests;

/// <summary>
/// Tests for DatabaseService data access methods (Get*, Save*, Clear*)
/// </summary>
public class DatabaseServiceDataTests : IDisposable
{
    private readonly string _testDbFolder;
    private readonly TestDeviceStorageService _mockStorage;
    private readonly TestUrlService _mockUrlService;
    private readonly DatabaseService _databaseService;
    private readonly string _databasePath;

    public DatabaseServiceDataTests()
    {
        _testDbFolder = Path.Combine(Path.GetTempPath(), $"SpiritualGiftsTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDbFolder);
        
        _databasePath = Path.Combine(_testDbFolder, "SpiritualGiftsSurvey.sqlite");
        
        _mockStorage = new TestDeviceStorageService(_databasePath);
        _mockUrlService = new TestUrlService();
        _databaseService = new DatabaseService(_mockStorage, _mockUrlService);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDbFolder))
        {
            Directory.Delete(_testDbFolder, true);
        }
        GC.SuppressFinalize(this);
    }

    #region GetQuestionsAsync Tests

    [Fact]
    public async Task GetQuestionsAsync_NoDatabase_ReturnsEmptyList()
    {
        // Arrange - no database exists
        
        // Act
        var questions = await _databaseService.GetQuestionsAsync("en");
        
        // Assert
        Assert.NotNull(questions);
        Assert.Empty(questions);
    }

    [Fact]
    public async Task GetQuestionsAsync_NoTranslation_ReturnsEmptyList()
    {
        // Arrange - database exists but no translation for language code
        await CreateTestDatabaseAsync();
        
        // Act
        var questions = await _databaseService.GetQuestionsAsync("invalid-code");
        
        // Assert
        Assert.NotNull(questions);
        Assert.Empty(questions);
    }

    [Fact]
    public async Task GetQuestionsAsync_ValidTranslation_ReturnsQuestions()
    {
        // Arrange
        var translationGuid = Guid.NewGuid();
        await CreateTestDatabaseAsync();
        await AddTestTranslationAsync("en", translationGuid);
        await AddTestQuestionsAsync(translationGuid, count: 5);
        
        // Act
        var questions = await _databaseService.GetQuestionsAsync("en");
        
        // Assert
        Assert.NotNull(questions);
        Assert.Equal(5, questions.Count);
        Assert.All(questions, q => Assert.Equal(translationGuid, q.TranslationGuid));
    }

    [Fact]
    public async Task GetQuestionsAsync_MultipleLanguages_ReturnsOnlyRequestedLanguage()
    {
        // Arrange
        var enGuid = Guid.NewGuid();
        var arGuid = Guid.NewGuid();
        await CreateTestDatabaseAsync();
        await AddTestTranslationAsync("en", enGuid);
        await AddTestTranslationAsync("ar", arGuid);
        await AddTestQuestionsAsync(enGuid, count: 3);
        await AddTestQuestionsAsync(arGuid, count: 2);
        
        // Act
        var enQuestions = await _databaseService.GetQuestionsAsync("en");
        var arQuestions = await _databaseService.GetQuestionsAsync("ar");
        
        // Assert
        Assert.Equal(3, enQuestions.Count);
        Assert.Equal(2, arQuestions.Count);
        Assert.All(enQuestions, q => Assert.Equal(enGuid, q.TranslationGuid));
        Assert.All(arQuestions, q => Assert.Equal(arGuid, q.TranslationGuid));
    }

    #endregion

    #region SaveUserGiftResultAsync Tests

    [Fact]
    public async Task SaveUserGiftResultAsync_ValidResult_SavesSuccessfully()
    {
        // Arrange
        CreateTestDatabase();
        var result = CreateTestSurveyResult();
        
        // Act
        await _databaseService.SaveUserGiftResultAsync(result);
        
        // Assert - verify result saved
        var savedResults = _databaseService.GetAllUserGiftResults();
        Assert.Single(savedResults);
        Assert.Equal(result.UserGiftResultGuid, savedResults[0].UserGiftResultGuid);
        Assert.Equal("John", savedResults[0].FirstName);
        Assert.Equal("Doe", savedResults[0].LastName);
    }

    [Fact]
    public async Task SaveUserGiftResultAsync_WithScores_SavesAllScores()
    {
        // Arrange
        CreateTestDatabase();
        var result = CreateTestSurveyResult();
        result.Scores = new List<UserGiftScore>
        {
            new() { GiftName = "Pastor", Score = 50, GiftRank = GiftRank.Primary },
            new() { GiftName = "Teaching", Score = 40, GiftRank = GiftRank.Secondary },
            new() { GiftName = "Evangelist", Score = 30, GiftRank = GiftRank.None }
        };
        
        // Act
        await _databaseService.SaveUserGiftResultAsync(result);
        
        // Assert - verify scores saved with correct UserGiftResultGuid
        using var conn = new SQLiteConnection(_databasePath);
        var scores = conn.Table<UserGiftScore>()
            .Where(s => s.UserGiftResultGuid == result.UserGiftResultGuid)
            .ToList();
        
        Assert.Equal(3, scores.Count);
        Assert.Contains(scores, s => s.GiftName == "Pastor" && s.Score == 50);
        Assert.Contains(scores, s => s.GiftName == "Teaching" && s.Score == 40);
        Assert.Contains(scores, s => s.GiftName == "Evangelist" && s.Score == 30);
    }

    [Fact]
    public async Task SaveUserGiftResultAsync_MultipleResults_SavesAll()
    {
        // Arrange
        CreateTestDatabase();
        var result1 = CreateTestSurveyResult();
        result1.FirstName = "Alice";
        var result2 = CreateTestSurveyResult();
        result2.FirstName = "Bob";
        
        // Act
        await _databaseService.SaveUserGiftResultAsync(result1);
        await _databaseService.SaveUserGiftResultAsync(result2);
        
        // Assert
        var savedResults = _databaseService.GetAllUserGiftResults();
        Assert.Equal(2, savedResults.Count);
        Assert.Contains(savedResults, r => r.FirstName == "Alice");
        Assert.Contains(savedResults, r => r.FirstName == "Bob");
    }

    #endregion

    #region GetAllUserGiftResults Tests

    [Fact]
    public void GetAllUserGiftResults_NoData_ReturnsEmptyList()
    {
        // Arrange
        CreateTestDatabase();
        
        // Act
        var results = _databaseService.GetAllUserGiftResults();
        
        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task GetAllUserGiftResults_HasData_ReturnsOrderedByDate()
    {
        // Arrange
        CreateTestDatabase();
        var old = CreateTestSurveyResult();
        old.DateTaken = DateTime.Now.AddDays(-2);
        old.FirstName = "Old";
        
        var recent = CreateTestSurveyResult();
        recent.DateTaken = DateTime.Now.AddDays(-1);
        recent.FirstName = "Recent";
        
        var newest = CreateTestSurveyResult();
        newest.DateTaken = DateTime.Now;
        newest.FirstName = "Newest";
        
        await _databaseService.SaveUserGiftResultAsync(old);
        await _databaseService.SaveUserGiftResultAsync(recent);
        await _databaseService.SaveUserGiftResultAsync(newest);
        
        // Act
        var results = _databaseService.GetAllUserGiftResults();
        
        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal("Newest", results[0].FirstName);
        Assert.Equal("Recent", results[1].FirstName);
        Assert.Equal("Old", results[2].FirstName);
    }

    #endregion

    #region ClearUserGiftDataAsync Tests

    [Fact]
    public async Task ClearUserGiftDataAsync_NoData_CompletesWithoutError()
    {
        // Arrange
        CreateTestDatabase();
        
        // Act
        await _databaseService.ClearUserGiftDataAsync();
        
        // Assert - no exception thrown
        Assert.True(true);
    }

    [Fact]
    public async Task ClearUserGiftDataAsync_HasData_DeletesAllResults()
    {
        // Arrange
        CreateTestDatabase();
        var result1 = CreateTestSurveyResult();
        var result2 = CreateTestSurveyResult();
        await _databaseService.SaveUserGiftResultAsync(result1);
        await _databaseService.SaveUserGiftResultAsync(result2);
        
        // Verify data exists
        Assert.Equal(2, _databaseService.GetAllUserGiftResults().Count);
        
        // Act
        await _databaseService.ClearUserGiftDataAsync();
        
        // Assert
        var results = _databaseService.GetAllUserGiftResults();
        Assert.Empty(results);
    }

    [Fact]
    public async Task ClearUserGiftDataAsync_HasScores_DeletesAllScores()
    {
        // Arrange
        CreateTestDatabase();
        var result = CreateTestSurveyResult();
        result.Scores = new List<UserGiftScore>
        {
            new() { GiftName = "Pastor", Score = 50 },
            new() { GiftName = "Teaching", Score = 40 }
        };
        await _databaseService.SaveUserGiftResultAsync(result);
        
        // Verify scores exist
        using (var conn = new SQLiteConnection(_databasePath))
        {
            var scoresBefore = conn.Table<UserGiftScore>().Count();
            Assert.Equal(2, scoresBefore);
        }
        
        // Act
        await _databaseService.ClearUserGiftDataAsync();
        
        // Assert
        using (var conn = new SQLiteConnection(_databasePath))
        {
            var scoresAfter = conn.Table<UserGiftScore>().Count();
            Assert.Equal(0, scoresAfter);
        }
    }

    #endregion

    #region GetGiftDescriptionsAsync Tests

    [Fact]
    public async Task GetGiftDescriptionsAsync_NoTranslation_ReturnsEmptyList()
    {
        // Arrange
        await CreateTestDatabaseAsync();
        
        // Act
        var descriptions = await _databaseService.GetGiftDescriptionsAsync("invalid");
        
        // Assert
        Assert.NotNull(descriptions);
        Assert.Empty(descriptions);
    }

    [Fact]
    public async Task GetGiftDescriptionsAsync_ValidTranslation_ReturnsDescriptions()
    {
        // Arrange
        var translationGuid = Guid.NewGuid();
        await CreateTestDatabaseAsync();
        await AddTestTranslationAsync("en", translationGuid);
        await AddTestGiftDescriptionsAsync(translationGuid, count: 3);
        
        // Act
        var descriptions = await _databaseService.GetGiftDescriptionsAsync("en");
        
        // Assert
        Assert.NotNull(descriptions);
        Assert.Equal(3, descriptions.Count);
        Assert.All(descriptions, d => Assert.Equal(translationGuid, d.TranslationGuid));
    }

    #endregion

    #region Helper Methods

    private void CreateTestDatabase()
    {
        using var conn = new SQLiteConnection(_databasePath);
        conn.CreateTable<Translation>();
        conn.CreateTable<Question>();
        conn.CreateTable<GiftDescription>();
        conn.CreateTable<Reflection>();
        conn.CreateTable<SurveyResult>();
        conn.CreateTable<UserGiftScore>();
        conn.CreateTable<DatabaseInfo>();
    }

    private async Task CreateTestDatabaseAsync()
    {
        var conn = new SQLiteAsyncConnection(_databasePath);
        try
        {
            await conn.CreateTableAsync<Translation>();
            await conn.CreateTableAsync<Question>();
            await conn.CreateTableAsync<GiftDescription>();
            await conn.CreateTableAsync<Reflection>();
            await conn.CreateTableAsync<SurveyResult>();
            await conn.CreateTableAsync<UserGiftScore>();
            await conn.CreateTableAsync<DatabaseInfo>();
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    private void AddTestTranslation(string code, Guid translationGuid)
    {
        using var conn = new SQLiteConnection(_databasePath);
        conn.Insert(new Translation
        {
            Code = code,
            TranslationGuid = translationGuid,
            FlowDirection = "LTR"
        });
    }

    private async Task AddTestTranslationAsync(string code, Guid translationGuid)
    {
        var conn = new SQLiteAsyncConnection(_databasePath);
        try
        {
            await conn.InsertAsync(new Translation
            {
                Code = code,
                TranslationGuid = translationGuid,
                FlowDirection = "LTR"
            });
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    private void AddTestQuestions(Guid translationGuid, int count)
    {
        using var conn = new SQLiteConnection(_databasePath);
        for (int i = 0; i < count; i++)
        {
            conn.Insert(new Question
            {
                QuestionGuid = Guid.NewGuid(),
                TranslationGuid = translationGuid,
                QuestionText = $"Question {i + 1}",
                Gift = (Gifts)(i % 10), // Cycle through gifts
                GiftDescriptionGuid = Guid.NewGuid()
            });
        }
    }

    private async Task AddTestQuestionsAsync(Guid translationGuid, int count)
    {
        var conn = new SQLiteAsyncConnection(_databasePath);
        try
        {
            for (int i = 0; i < count; i++)
            {
                await conn.InsertAsync(new Question
                {
                    QuestionGuid = Guid.NewGuid(),
                    TranslationGuid = translationGuid,
                    QuestionText = $"Question {i + 1}",
                    Gift = (Gifts)(i % 10),
                    GiftDescriptionGuid = Guid.NewGuid()
                });
            }
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    private void AddTestGiftDescriptions(Guid translationGuid, int count)
    {
        using var conn = new SQLiteConnection(_databasePath);
        for (int i = 0; i < count; i++)
        {
            conn.Insert(new GiftDescription
            {
                GiftDescriptionGuid = Guid.NewGuid(),
                TranslationGuid = translationGuid,
                Gift = (Gifts)(i % 10),
                Translation = $"Gift {i}",
                Description = $"Description {i}"
            });
        }
    }

    private async Task AddTestGiftDescriptionsAsync(Guid translationGuid, int count)
    {
        var conn = new SQLiteAsyncConnection(_databasePath);
        try
        {
            for (int i = 0; i < count; i++)
            {
                await conn.InsertAsync(new GiftDescription
                {
                    GiftDescriptionGuid = Guid.NewGuid(),
                    TranslationGuid = translationGuid,
                    Gift = (Gifts)(i % 10),
                    Translation = $"Gift {i}",
                    Description = $"Description {i}"
                });
            }
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    private SurveyResult CreateTestSurveyResult()
    {
        return new SurveyResult
        {
            UserGiftResultGuid = Guid.NewGuid(),
            DateTaken = DateTime.Now,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Results = "Test results"
        };
    }

    private class TestDeviceStorageService : IDeviceStorageService
    {
        private readonly string _databasePath;

        public TestDeviceStorageService(string databasePath)
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

    private class TestUrlService : IUrlService
    {
        public async Task<Result<int>> GetRemoteDatabaseVersionAsync()
        {
            await Task.CompletedTask;
            return new Result<int>(1);
        }

        public async Task<Result<RootModel>> GetFullDatabaseAsync()
        {
            await Task.CompletedTask;
            return new Result<RootModel>(new TitledException("Not implemented", "Not needed for data tests"));
        }
    }

    #endregion

    #region GetTranslationGuid Tests

    [Fact]
    public void GetTranslationGuid_NoDatabase_ThrowsOrReturnsEmptyGuid()
    {
        // Arrange - no database exists
        
        // Act & Assert - method throws SQLiteException when no database exists
        // This is expected behavior for this low-level method
        var exception = Record.Exception(() => _databaseService.GetTranslationGuid("en"));
        Assert.True(exception is SQLiteException || exception == null);
    }

    [Fact]
    public void GetTranslationGuid_NoTranslation_ReturnsEmptyGuid()
    {
        // Arrange - database exists but no translation for language code
        CreateTestDatabase();
        
        // Act
        var guid = _databaseService.GetTranslationGuid("invalid-code");
        
        // Assert
        Assert.Equal(Guid.Empty, guid);
    }

    [Fact]
    public void GetTranslationGuid_ValidTranslation_ReturnsGuid()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        CreateTestDatabase();
        AddTestTranslation("en", expectedGuid);
        
        // Act
        var actualGuid = _databaseService.GetTranslationGuid("en");
        
        // Assert
        Assert.Equal(expectedGuid, actualGuid);
    }

    [Fact]
    public void GetTranslationGuid_CaseInsensitive_ReturnsGuid()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        CreateTestDatabase();
        AddTestTranslation("en", expectedGuid);
        
        // Act - test with different casing
        var guidUpper = _databaseService.GetTranslationGuid("EN");
        var guidMixed = _databaseService.GetTranslationGuid("En");
        
        // Assert
        Assert.Equal(expectedGuid, guidUpper);
        Assert.Equal(expectedGuid, guidMixed);
    }

    #endregion

    #region GetQuestionsCountAsync Tests

    [Fact]
    public async Task GetQuestionsCountAsync_NoDatabase_ThrowsException()
    {
        // Arrange - no database exists
        
        // Act & Assert - method throws when database doesn't exist
        // This is expected behavior for this low-level method
        await Assert.ThrowsAsync<SQLiteException>(async () => 
            await _databaseService.GetQuestionsCountAsync("en"));
    }

    [Fact]
    public async Task GetQuestionsCountAsync_NoTranslation_ReturnsZero()
    {
        // Arrange - database exists but no translation for language code
        await CreateTestDatabaseAsync();
        
        // Act
        var count = await _databaseService.GetQuestionsCountAsync("invalid-code");
        
        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task GetQuestionsCountAsync_ValidTranslation_ReturnsCorrectCount()
    {
        // Arrange
        var translationGuid = Guid.NewGuid();
        await CreateTestDatabaseAsync();
        await AddTestTranslationAsync("en", translationGuid);
        await AddTestQuestionsAsync(translationGuid, count: 250);
        
        // Act
        var count = await _databaseService.GetQuestionsCountAsync("en");
        
        // Assert
        Assert.Equal(250, count);
    }

    [Fact]
    public async Task GetQuestionsCountAsync_MultipleLanguages_ReturnsOnlyRequestedCount()
    {
        // Arrange
        var enGuid = Guid.NewGuid();
        var arGuid = Guid.NewGuid();
        await CreateTestDatabaseAsync();
        await AddTestTranslationAsync("en", enGuid);
        await AddTestTranslationAsync("ar", arGuid);
        await AddTestQuestionsAsync(enGuid, count: 100);
        await AddTestQuestionsAsync(arGuid, count: 50);
        
        // Act
        var enCount = await _databaseService.GetQuestionsCountAsync("en");
        var arCount = await _databaseService.GetQuestionsCountAsync("ar");
        
        // Assert
        Assert.Equal(100, enCount);
        Assert.Equal(50, arCount);
    }

    [Fact]
    public async Task GetQuestionsCountAsync_EmptyQuestionSet_ReturnsZero()
    {
        // Arrange - translation exists but no questions
        var translationGuid = Guid.NewGuid();
        await CreateTestDatabaseAsync();
        await AddTestTranslationAsync("en", translationGuid);
        // No questions added
        
        // Act
        var count = await _databaseService.GetQuestionsCountAsync("en");
        
        // Assert
        Assert.Equal(0, count);
    }

    #endregion

    #region GetGiftDescription (string, Gifts) Overload Tests

    [Fact]
    public void GetGiftDescription_ByLanguageAndGift_NoDatabase_ThrowsException()
    {
        // Arrange - no database exists
        
        // Act & Assert - method throws when database doesn't exist
        // This is expected behavior for this low-level method
        Assert.Throws<SQLiteException>(() => 
            _databaseService.GetGiftDescription("en", Gifts.Prophecy));
    }

    [Fact]
    public void GetGiftDescription_ByLanguageAndGift_NoTranslation_ReturnsNull()
    {
        // Arrange - database exists but no translation for language code
        CreateTestDatabase();
        
        // Act
        var description = _databaseService.GetGiftDescription("invalid-code", Gifts.Prophecy);
        
        // Assert
        Assert.Null(description);
    }

    [Fact]
    public void GetGiftDescription_ByLanguageAndGift_NoMatchingGift_ReturnsNull()
    {
        // Arrange
        var translationGuid = Guid.NewGuid();
        CreateTestDatabase();
        AddTestTranslation("en", translationGuid);
        
        // Add gift description for Teaching, but query for Prophecy
        using var conn = new SQLiteConnection(_databasePath);
        conn.Insert(new GiftDescription
        {
            GiftDescriptionGuid = Guid.NewGuid(),
            TranslationGuid = translationGuid,
            Gift = Gifts.Teaching,
            Translation = "Teaching",
            Description = "The gift of teaching"
        });
        
        // Act
        var description = _databaseService.GetGiftDescription("en", Gifts.Prophecy);
        
        // Assert
        Assert.Null(description);
    }

    [Fact]
    public void GetGiftDescription_ByLanguageAndGift_ValidMatch_ReturnsDescription()
    {
        // Arrange
        var translationGuid = Guid.NewGuid();
        var expectedGuid = Guid.NewGuid();
        CreateTestDatabase();
        AddTestTranslation("en", translationGuid);
        
        using var conn = new SQLiteConnection(_databasePath);
        conn.Insert(new GiftDescription
        {
            GiftDescriptionGuid = expectedGuid,
            TranslationGuid = translationGuid,
            Gift = Gifts.Prophecy,
            Translation = "Prophecy",
            Description = "The gift of prophecy"
        });
        
        // Act
        var description = _databaseService.GetGiftDescription("en", Gifts.Prophecy);
        
        // Assert
        Assert.NotNull(description);
        Assert.Equal(expectedGuid, description.GiftDescriptionGuid);
        Assert.Equal(Gifts.Prophecy, description.Gift);
        Assert.Equal("Prophecy", description.Translation);
        Assert.Equal("The gift of prophecy", description.Description);
    }

    [Fact]
    public void GetGiftDescription_ByLanguageAndGift_MultipleLanguages_ReturnsCorrectLanguage()
    {
        // Arrange
        var enGuid = Guid.NewGuid();
        var arGuid = Guid.NewGuid();
        var enDescGuid = Guid.NewGuid();
        var arDescGuid = Guid.NewGuid();
        
        CreateTestDatabase();
        AddTestTranslation("en", enGuid);
        AddTestTranslation("ar", arGuid);
        
        using var conn = new SQLiteConnection(_databasePath);
        conn.Insert(new GiftDescription
        {
            GiftDescriptionGuid = enDescGuid,
            TranslationGuid = enGuid,
            Gift = Gifts.Healing,
            Translation = "Healing",
            Description = "The gift of healing"
        });
        conn.Insert(new GiftDescription
        {
            GiftDescriptionGuid = arDescGuid,
            TranslationGuid = arGuid,
            Gift = Gifts.Healing,
            Translation = "شفاء",
            Description = "موهبة الشفاء"
        });
        
        // Act
        var enDescription = _databaseService.GetGiftDescription("en", Gifts.Healing);
        var arDescription = _databaseService.GetGiftDescription("ar", Gifts.Healing);
        
        // Assert
        Assert.NotNull(enDescription);
        Assert.NotNull(arDescription);
        Assert.Equal(enDescGuid, enDescription.GiftDescriptionGuid);
        Assert.Equal(arDescGuid, arDescription.GiftDescriptionGuid);
        Assert.Equal("Healing", enDescription.Translation);
        Assert.Equal("شفاء", arDescription.Translation);
    }

    [Fact]
    public void GetGiftDescription_ByLanguageAndGift_CaseInsensitive_ReturnsDescription()
    {
        // Arrange
        var translationGuid = Guid.NewGuid();
        CreateTestDatabase();
        AddTestTranslation("en", translationGuid);
        
        using var conn = new SQLiteConnection(_databasePath);
        conn.Insert(new GiftDescription
        {
            GiftDescriptionGuid = Guid.NewGuid(),
            TranslationGuid = translationGuid,
            Gift = Gifts.Wisdom,
            Translation = "Wisdom",
            Description = "The gift of wisdom"
        });
        
        // Act - test with different casing
        var descUpper = _databaseService.GetGiftDescription("EN", Gifts.Wisdom);
        var descMixed = _databaseService.GetGiftDescription("En", Gifts.Wisdom);
        
        // Assert
        Assert.NotNull(descUpper);
        Assert.NotNull(descMixed);
        Assert.Equal("Wisdom", descUpper.Translation);
        Assert.Equal("Wisdom", descMixed.Translation);
    }

    #endregion

    #region GetReflections Tests

    [Fact]
    public void GetReflections_NoDatabase_ThrowsException()
    {
        // Arrange - no database exists
        
        // Act & Assert - method throws when database doesn't exist
        // This is expected behavior for this low-level method
        Assert.Throws<SQLiteException>(() => 
            _databaseService.GetReflections("en"));
    }

    [Fact]
    public void GetReflections_NoTranslation_ReturnsEmptyList()
    {
        // Arrange - database exists but no translation for language code
        CreateTestDatabase();
        
        // Act
        var reflections = _databaseService.GetReflections("invalid-code");
        
        // Assert
        Assert.NotNull(reflections);
        Assert.Empty(reflections);
    }

    [Fact]
    public void GetReflections_ValidTranslation_ReturnsReflections()
    {
        // Arrange
        var translationGuid = Guid.NewGuid();
        CreateTestDatabase();
        AddTestTranslation("en", translationGuid);
        AddTestReflections(translationGuid, count: 5);
        
        // Act
        var reflections = _databaseService.GetReflections("en");
        
        // Assert
        Assert.NotNull(reflections);
        Assert.Equal(5, reflections.Count);
        Assert.All(reflections, r => Assert.Equal(translationGuid, r.TranslationGuid));
    }

    [Fact]
    public void GetReflections_MultipleLanguages_ReturnsOnlyRequestedLanguage()
    {
        // Arrange
        var enGuid = Guid.NewGuid();
        var arGuid = Guid.NewGuid();
        CreateTestDatabase();
        AddTestTranslation("en", enGuid);
        AddTestTranslation("ar", arGuid);
        AddTestReflections(enGuid, count: 3);
        AddTestReflections(arGuid, count: 2);
        
        // Act
        var enReflections = _databaseService.GetReflections("en");
        var arReflections = _databaseService.GetReflections("ar");
        
        // Assert
        Assert.Equal(3, enReflections.Count);
        Assert.Equal(2, arReflections.Count);
        Assert.All(enReflections, r => Assert.Equal(enGuid, r.TranslationGuid));
        Assert.All(arReflections, r => Assert.Equal(arGuid, r.TranslationGuid));
    }

    [Fact]
    public void GetReflections_EmptyReflectionSet_ReturnsEmptyList()
    {
        // Arrange - translation exists but no reflections
        var translationGuid = Guid.NewGuid();
        CreateTestDatabase();
        AddTestTranslation("en", translationGuid);
        // No reflections added
        
        // Act
        var reflections = _databaseService.GetReflections("en");
        
        // Assert
        Assert.NotNull(reflections);
        Assert.Empty(reflections);
    }

    [Fact]
    public void GetReflections_ReflectionContent_VerifyFields()
    {
        // Arrange
        var translationGuid = Guid.NewGuid();
        var reflectionGuid = Guid.NewGuid();
        CreateTestDatabase();
        AddTestTranslation("en", translationGuid);
        
        using var conn = new SQLiteConnection(_databasePath);
        conn.Insert(new Reflection
        {
            ReflectionGuid = reflectionGuid,
            TranslationGuid = translationGuid,
            Number = 42,
            Question = "How does the gift of mercy manifest in daily life?"
        });
        
        // Act
        var reflections = _databaseService.GetReflections("en");
        
        // Assert
        Assert.Single(reflections);
        var reflection = reflections[0];
        Assert.Equal(reflectionGuid, reflection.ReflectionGuid);
        Assert.Equal(42, reflection.Number);
        Assert.Equal("How does the gift of mercy manifest in daily life?", reflection.Question);
    }

    #endregion

    #region Additional Helper Methods

    private void AddTestReflections(Guid translationGuid, int count)
    {
        using var conn = new SQLiteConnection(_databasePath);
        for (int i = 0; i < count; i++)
        {
            conn.Insert(new Reflection
            {
                ReflectionGuid = Guid.NewGuid(),
                TranslationGuid = translationGuid,
                Number = i + 1,
                Question = $"Reflection Question {i + 1}?"
            });
        }
    }

    #endregion
}

