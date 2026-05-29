using System.Text.Json;
using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Services;

namespace SpiritualGiftsSurvey.Tests;

/// <summary>
/// Regression tests for JSON source generation (CRITICAL-2).
/// Validates AppJsonContext works correctly for RootModel and List<string>.
/// </summary>
public class JsonSourceGenerationTests
{
    [Fact]
    public void AppJsonContext_DeserializesRootModel_Successfully()
    {
        // Arrange
        var json = @"{
            ""database"": {
                ""date"": ""2025-01-15"",
                ""version"": 1,
                ""author"": ""Test"",
                ""environment"": ""Test"",
                ""notes"": ""Test data""
            },
            ""translations"": [
                {
                    ""id"": 1,
                    ""translationGuid"": ""00000000-0000-0000-0000-000000000001"",
                    ""code"": ""en"",
                    ""flowDirection"": ""LTR""
                }
            ]
        }";

        // Act
        var model = JsonSerializer.Deserialize(json, AppJsonContext.Default.RootModel);

        // Assert
        Assert.NotNull(model);
        Assert.NotNull(model.Database);
        Assert.Equal("2025-01-15", model.Database.Date);
        Assert.Equal(1, model.Database.Version);
        Assert.NotNull(model.Translations);
        Assert.Single(model.Translations);
        Assert.Equal("en", model.Translations[0].Code);
    }

    [Fact]
    public void AppJsonContext_SerializesRootModel_Successfully()
    {
        // Arrange
        var model = new RootModel
        {
            Database = new DatabaseInfo
            {
                Date = "2025-01-15",
                Version = 1,
                Author = "Test"
            },
            Translations = new List<Translation>
            {
                new() { Code = "en", FlowDirection = "LTR" }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(model, AppJsonContext.Default.RootModel);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"date\"", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"version\"", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"en\"", json);
    }

    [Fact]
    public void AppJsonContext_DeserializesListString_Successfully()
    {
        // Arrange
        var json = @"[""email1@test.com"", ""email2@test.com"", ""email3@test.com""]";

        // Act
        var list = JsonSerializer.Deserialize(json, AppJsonContext.Default.ListString);

        // Assert
        Assert.NotNull(list);
        Assert.Equal(3, list.Count);
        Assert.Equal("email1@test.com", list[0]);
        Assert.Equal("email2@test.com", list[1]);
        Assert.Equal("email3@test.com", list[2]);
    }

    [Fact]
    public void AppJsonContext_SerializesListString_Successfully()
    {
        // Arrange
        var list = new List<string> { "email1@test.com", "email2@test.com" };

        // Act
        var json = JsonSerializer.Serialize(list, AppJsonContext.Default.ListString);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("email1@test.com", json);
        Assert.Contains("email2@test.com", json);
    }

    [Fact]
    public void AppJsonContext_HandlesEmptyList_Successfully()
    {
        // Arrange
        var emptyList = new List<string>();

        // Act
        var json = JsonSerializer.Serialize(emptyList, AppJsonContext.Default.ListString);
        var deserialized = JsonSerializer.Deserialize(json, AppJsonContext.Default.ListString);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Empty(deserialized);
    }

    [Fact]
    public void AppJsonContext_PropertyNameCaseInsensitive_Works()
    {
        // Arrange - JSON with different casing
        var json = @"{
            ""Database"": {
                ""DATE"": ""2025-01-15"",
                ""VERSION"": 1
            },
            ""TRANSLATIONS"": []
        }";

        // Act
        var model = JsonSerializer.Deserialize(json, AppJsonContext.Default.RootModel);

        // Assert - Should deserialize despite case differences
        Assert.NotNull(model);
        Assert.NotNull(model.Database);
        Assert.Equal("2025-01-15", model.Database.Date);
        Assert.Equal(1, model.Database.Version);
    }

    [Fact]
    public void AppJsonContext_HandlesNullTranslations_Gracefully()
    {
        // Arrange
        var json = @"{
            ""database"": {
                ""date"": ""2025-01-15"",
                ""version"": 1
            },
            ""translations"": null
        }";

        // Act
        var model = JsonSerializer.Deserialize(json, AppJsonContext.Default.RootModel);

        // Assert
        Assert.NotNull(model);
        Assert.Null(model.Translations); // Should handle null gracefully
    }
}
