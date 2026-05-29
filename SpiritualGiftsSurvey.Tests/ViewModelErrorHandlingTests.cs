using System.Net;
using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Utilities;

namespace SpiritualGiftsSurvey.Tests;

/// <summary>
/// Integration tests for error handling and timeout protection in network operations.
/// Validates that the app gracefully handles network failures, timeouts, and edge cases.
/// 
/// CRITICAL: These tests verify error resilience in Firebase operations.
/// If these fail, the app may hang indefinitely on network issues.
/// </summary>
public class ViewModelErrorHandlingTests
{
    [Fact]
    public async Task UrlService_GetFullDatabase_TimesOut_ReturnsErrorWithinTimeout()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(delayMs: 35000); // Delay longer than 30s timeout
        var httpClient = new HttpClient(mockHandler)
        {
            BaseAddress = new Uri("https://test.firebaseio.com/")
        };
        var urlService = new UrlService(httpClient);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await urlService.GetFullDatabaseAsync();
        stopwatch.Stop();

        // Assert
        Assert.False(result.IsSuccess, "Request should fail due to timeout");
        Assert.NotNull(result.Error);
        Assert.Contains("timed out", result.Error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.True(stopwatch.Elapsed.TotalSeconds < 35, $"Should timeout before 35s, but took {stopwatch.Elapsed.TotalSeconds}s");
        Assert.True(stopwatch.Elapsed.TotalSeconds >= 29, $"Should wait at least 29s before timeout, but took {stopwatch.Elapsed.TotalSeconds}s");
    }

    [Fact]
    public async Task UrlService_GetFullDatabase_NetworkError_ReturnsError()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(throwException: true);
        var httpClient = new HttpClient(mockHandler)
        {
            BaseAddress = new Uri("https://test.firebaseio.com/")
        };
        var urlService = new UrlService(httpClient);

        // Act
        var result = await urlService.GetFullDatabaseAsync();

        // Assert
        Assert.False(result.IsSuccess, "Request should fail due to network error");
        Assert.NotNull(result.Error);
        Assert.Contains("Network Error", result.Error.ExceptionTitle, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UrlService_GetRemoteDatabaseVersion_TimesOut_ReturnsError()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(delayMs: 35000);
        var httpClient = new HttpClient(mockHandler)
        {
            BaseAddress = new Uri("https://test.firebaseio.com/")
        };
        var urlService = new UrlService(httpClient);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await urlService.GetRemoteDatabaseVersionAsync();
        stopwatch.Stop();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("timed out", result.Error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.True(stopwatch.Elapsed.TotalSeconds < 35, "Should timeout before mock delay completes");
    }

    [Fact]
    public async Task UrlService_GetFullDatabase_SuccessfulResponse_ReturnsData()
    {
        // Arrange
        var validJson = @"{
            ""Database"": {
                ""Version"": 1,
                ""Date"": ""2025-01-01"",
                ""Author"": ""Test"",
                ""Environment"": ""Test"",
                ""Notes"": ""Test database""
            },
            ""Translations"": []
        }";
        var mockHandler = new MockHttpMessageHandler(responseContent: validJson);
        var httpClient = new HttpClient(mockHandler)
        {
            BaseAddress = new Uri("https://test.firebaseio.com/")
        };
        var urlService = new UrlService(httpClient);

        // Act
        var result = await urlService.GetFullDatabaseAsync();

        // Assert
        Assert.True(result.IsSuccess, "Request should succeed with valid JSON");
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value.Database);
        Assert.Equal(1, result.Value.Database.Version);
    }

    [Fact]
    public async Task UrlService_GetFullDatabase_EmptyResponse_ReturnsError()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(responseContent: "");
        var httpClient = new HttpClient(mockHandler)
        {
            BaseAddress = new Uri("https://test.firebaseio.com/")
        };
        var urlService = new UrlService(httpClient);

        // Act
        var result = await urlService.GetFullDatabaseAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("Empty Response", result.Error.Message);
    }

    [Fact]
    public async Task UrlService_GetFullDatabase_InvalidJson_ReturnsError()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(responseContent: "{invalid json}");
        var httpClient = new HttpClient(mockHandler)
        {
            BaseAddress = new Uri("https://test.firebaseio.com/")
        };
        var urlService = new UrlService(httpClient);

        // Act
        var result = await urlService.GetFullDatabaseAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("Failed to parse JSON", result.Error.Message);
    }

    [Fact]
    public async Task UrlService_GetRemoteDatabaseVersion_ValidVersion_ReturnsInt()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(responseContent: "42");
        var httpClient = new HttpClient(mockHandler)
        {
            BaseAddress = new Uri("https://test.firebaseio.com/")
        };
        var urlService = new UrlService(httpClient);

        // Act
        var result = await urlService.GetRemoteDatabaseVersionAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task UrlService_GetRemoteDatabaseVersion_InvalidVersion_ReturnsError()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(responseContent: "not-a-number");
        var httpClient = new HttpClient(mockHandler)
        {
            BaseAddress = new Uri("https://test.firebaseio.com/")
        };
        var urlService = new UrlService(httpClient);

        // Act
        var result = await urlService.GetRemoteDatabaseVersionAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("Failed to parse version", result.Error.Message);
    }
}

#region Mock HTTP Handler

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly int _delayMs;
    private readonly bool _throwException;
    private readonly string _responseContent;

    public MockHttpMessageHandler(
        int delayMs = 0,
        bool throwException = false,
        string responseContent = "{}")
    {
        _delayMs = delayMs;
        _throwException = throwException;
        _responseContent = responseContent;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (_throwException)
            throw new HttpRequestException("Mock network error");

        if (_delayMs > 0)
            await Task.Delay(_delayMs, cancellationToken);

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(_responseContent)
        };
    }
}

#endregion
