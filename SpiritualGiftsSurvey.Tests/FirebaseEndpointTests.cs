using System.Text.Json;
using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Services;

namespace SpiritualGiftsSurvey.Tests;

/// <summary>
/// Integration tests for Firebase Realtime Database endpoint connectivity.
/// Validates that the production and dev endpoints are reachable and returning valid data.
/// 
/// CRITICAL: These tests verify actual network connectivity to Firebase.
/// If these fail, the database sync feature will NOT work in the app.
/// </summary>
public class FirebaseEndpointTests
{
    private const string ProdBaseUrl = "https://sgt-prod-691ce-default-rtdb.firebaseio.com/";
    private const string DevBaseUrl = "https://sgt-dev-b29c8-default-rtdb.firebaseio.com/";

    [Fact]
    public async Task ProductionEndpoint_IsReachable()
    {
        // Arrange
        using var client = new HttpClient();
        var uri = new Uri($"{ProdBaseUrl}/.json");

        // Act
        var response = await client.GetAsync(uri);

        // Assert
        Assert.True(response.IsSuccessStatusCode, 
            $"❌ PRODUCTION Firebase endpoint UNREACHABLE. Status: {response.StatusCode}\nThis explains why database is not updating!");
    }

    [Fact]
    public async Task ProductionEndpoint_ReturnsValidJson()
    {
        // Arrange
        using var client = new HttpClient();
        var uri = new Uri($"{ProdBaseUrl}/.json");

        // Act
        var response = await client.GetAsync(uri);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        Assert.NotEqual("null", content); // Firebase returns "null" for missing/empty data
    }

    [Fact]
    public async Task ProductionEndpoint_ReturnsValidRootModel()
    {
        // Arrange
        using var client = new HttpClient();
        var uri = new Uri($"{ProdBaseUrl}/.json");

        // Act
        var response = await client.GetAsync(uri);
        var json = await response.Content.ReadAsStringAsync();
        
        RootModel? data = null;
        Exception? deserializationException = null;
        try
        {
            data = JsonSerializer.Deserialize(json, AppJsonContext.Default.RootModel);
        }
        catch (Exception ex)
        {
            deserializationException = ex;
        }

        // Assert with payload capture
        if (deserializationException != null)
        {
            // Capture the JSON payload for debugging
            var payloadPreview = json.Length > 1000 
                ? json.Substring(0, 1000) + "\n... (truncated, total length: " + json.Length + " chars)" 
                : json;
            
            Assert.Fail($"❌ PRODUCTION Firebase JSON deserialization FAILED.\n" +
                       $"Exception: {deserializationException.Message}\n" +
                       $"JSON Payload Preview:\n{payloadPreview}\n\n" +
                       $"Full Stack Trace:\n{deserializationException}");
        }
        
        Assert.NotNull(data);
        Assert.NotNull(data.Database);
        Assert.NotNull(data.Translations);
        Assert.NotEmpty(data.Translations);
    }

    [Fact]
    public async Task ProductionEndpoint_DatabaseVersionIsValid()
    {
        // Arrange
        using var client = new HttpClient();
        var versionUri = new Uri($"{ProdBaseUrl}/Database/Version.json");

        // Act
        var response = await client.GetAsync(versionUri);
        var versionJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(versionJson);
        Assert.NotEqual("null", versionJson);
        
        var version = int.Parse(versionJson);
        Assert.True(version > 0, $"Database version should be > 0, got {version}");
    }

    [Fact]
    public async Task ProductionEndpoint_HasRequiredData()
    {
        // Arrange
        using var client = new HttpClient();
        var uri = new Uri($"{ProdBaseUrl}/.json");

        // Act
        var response = await client.GetAsync(uri);
        var json = await response.Content.ReadAsStringAsync();
        
        RootModel? data = null;
        try
        {
            data = JsonSerializer.Deserialize(json, AppJsonContext.Default.RootModel);
        }
        catch (Exception ex)
        {
            // Capture the JSON payload for debugging
            var payloadPreview = json.Length > 1000 
                ? json.Substring(0, 1000) + "\n... (truncated, total length: " + json.Length + " chars)" 
                : json;
            
            Assert.Fail($"❌ PRODUCTION Firebase JSON deserialization FAILED.\n" +
                       $"Exception: {ex.Message}\n" +
                       $"JSON Payload Preview:\n{payloadPreview}\n\n" +
                       $"Full Stack Trace:\n{ex}");
        }

        // Assert
        Assert.NotNull(data);

        // Validate Database metadata
        Assert.NotNull(data.Database);
        Assert.NotNull(data.Database.Date);
        Assert.True(data.Database.Version > 0, $"Database version should be > 0, got {data.Database.Version}");

        // Validate Translations exist
        Assert.NotNull(data.Translations);
        Assert.NotEmpty(data.Translations);

        // Validate at least one translation exists with proper structure
        var firstTranslation = data.Translations.First();
        Assert.NotNull(firstTranslation.Code);
        Assert.NotEmpty(firstTranslation.Code);
        
        // Validate first translation has Questions
        Assert.NotNull(firstTranslation.Questions);
        Assert.NotEmpty(firstTranslation.Questions);
        Assert.True(firstTranslation.Questions.Count >= 10, 
            $"Expected at least 10 questions, found {firstTranslation.Questions.Count}");

        // Validate first translation has GiftDescriptions
        Assert.NotNull(firstTranslation.GiftDescriptions);
        Assert.NotEmpty(firstTranslation.GiftDescriptions);
        Assert.True(firstTranslation.GiftDescriptions.Count >= 10,
            $"Expected at least 10 gift descriptions, found {firstTranslation.GiftDescriptions.Count}");
    }

    [Fact]
    public async Task DevEndpoint_IsReachable()
    {
        // Arrange
        using var client = new HttpClient();
        var uri = new Uri($"{DevBaseUrl}/.json");

        // Act
        var response = await client.GetAsync(uri);

        // Assert
        Assert.True(response.IsSuccessStatusCode, 
            $"❌ DEV Firebase endpoint UNREACHABLE. Status: {response.StatusCode}");
    }

    [Fact]
    public async Task DevEndpoint_ReturnsValidJson()
    {
        // Arrange
        using var client = new HttpClient();
        var uri = new Uri($"{DevBaseUrl}/.json");

        // Act
        var response = await client.GetAsync(uri);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        Assert.NotEqual("null", content);
    }

    [Fact]
    public async Task DevEndpoint_ReturnsValidRootModel()
    {
        // Arrange
        using var client = new HttpClient();
        var uri = new Uri($"{DevBaseUrl}/.json");

        // Act
        var response = await client.GetAsync(uri);
        var json = await response.Content.ReadAsStringAsync();
        
        RootModel? data = null;
        Exception? deserializationException = null;
        try
        {
            data = JsonSerializer.Deserialize(json, AppJsonContext.Default.RootModel);
        }
        catch (Exception ex)
        {
            deserializationException = ex;
        }

        // Assert with payload capture
        if (deserializationException != null)
        {
            // Capture the JSON payload for debugging
            var payloadPreview = json.Length > 1000 
                ? json.Substring(0, 1000) + "\n... (truncated, total length: " + json.Length + " chars)" 
                : json;
            
            Assert.Fail($"❌ DEV Firebase JSON deserialization FAILED.\n" +
                       $"Exception: {deserializationException.Message}\n" +
                       $"JSON Payload Preview:\n{payloadPreview}\n\n" +
                       $"Full Stack Trace:\n{deserializationException}");
        }
        
        Assert.NotNull(data);
        Assert.NotNull(data.Database);
        Assert.NotNull(data.Translations);
        Assert.NotEmpty(data.Translations);
    }

    [Fact]
    public async Task DevEndpoint_DatabaseVersionIsValid()
    {
        // Arrange
        using var client = new HttpClient();
        var versionUri = new Uri($"{DevBaseUrl}/Database/Version.json");

        // Act
        var response = await client.GetAsync(versionUri);
        var versionJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(versionJson);
        Assert.NotEqual("null", versionJson);
        
        var version = int.Parse(versionJson);
        Assert.True(version > 0, $"Database version should be > 0, got {version}");
    }

    [Fact]
    public async Task DevEndpoint_HasRequiredData()
    {
        // Arrange
        using var client = new HttpClient();
        var uri = new Uri($"{DevBaseUrl}/.json");

        // Act
        var response = await client.GetAsync(uri);
        var json = await response.Content.ReadAsStringAsync();
        
        RootModel? data = null;
        try
        {
            data = JsonSerializer.Deserialize(json, AppJsonContext.Default.RootModel);
        }
        catch (Exception ex)
        {
            // Capture the JSON payload for debugging
            var payloadPreview = json.Length > 1000 
                ? json.Substring(0, 1000) + "\n... (truncated, total length: " + json.Length + " chars)" 
                : json;
            
            Assert.Fail($"❌ DEV Firebase JSON deserialization FAILED.\n" +
                       $"Exception: {ex.Message}\n" +
                       $"JSON Payload Preview:\n{payloadPreview}\n\n" +
                       $"Full Stack Trace:\n{ex}");
        }

        // Assert
        Assert.NotNull(data);

        // Validate Database metadata
        Assert.NotNull(data.Database);
        Assert.NotNull(data.Database.Date);
        Assert.True(data.Database.Version > 0, $"Database version should be > 0, got {data.Database.Version}");

        // Validate Translations exist
        Assert.NotNull(data.Translations);
        Assert.NotEmpty(data.Translations);

        // Validate at least one translation exists with proper structure
        var firstTranslation = data.Translations.First();
        Assert.NotNull(firstTranslation.Code);
        Assert.NotEmpty(firstTranslation.Code);
        
        // Validate first translation has Questions
        Assert.NotNull(firstTranslation.Questions);
        Assert.NotEmpty(firstTranslation.Questions);
        Assert.True(firstTranslation.Questions.Count >= 10, 
            $"Expected at least 10 questions, found {firstTranslation.Questions.Count}");

        // Validate first translation has GiftDescriptions
        Assert.NotNull(firstTranslation.GiftDescriptions);
        Assert.NotEmpty(firstTranslation.GiftDescriptions);
        Assert.True(firstTranslation.GiftDescriptions.Count >= 10,
            $"Expected at least 10 gift descriptions, found {firstTranslation.GiftDescriptions.Count}");
    }

    [Fact]
    public async Task NetworkError_IsHandledGracefully()
    {
        // Arrange - Invalid URL to simulate network error
        using var client = new HttpClient();
        var uri = new Uri("https://invalid-firebase-url-12345.firebaseio.com/.json");

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () => 
        {
            var response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
        });
    }
}
