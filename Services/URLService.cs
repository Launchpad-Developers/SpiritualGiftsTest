using CommunityToolkit.Mvvm.ComponentModel;
using SpiritualGiftsSurvey.Enums;
using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Utilities;
using System.Text.Json;

namespace SpiritualGiftsSurvey.Services;

public interface IUrlService
{
    Task<Result<RootModel>> GetFullDatabaseAsync();
    Task<Result<int>> GetRemoteDatabaseVersionAsync();
}

public class Result<T>
{
    public bool IsSuccess => Error == null;
    public T? Value { get; }
    public TitledException? Error { get; }

    public Result(T value) => Value = value;
    public Result(TitledException error) => Error = error;

    public T ValueOrThrow() => Value ?? throw new InvalidOperationException("No value present");
}

public partial class UrlService : ObservableObject, IUrlService
{
    private readonly HttpClient _client;

    public AppEnvironment Environment { get; }

    public string BaseURL => _client.BaseAddress?.ToString().TrimEnd('/')
                             ?? throw new InvalidOperationException("BaseUrl is not set.");

    public UrlService(HttpClient client)
    {
        _client = client;
    }

    public async Task<Result<RootModel>> GetFullDatabaseAsync()
    {
        Console.WriteLine($"[UrlService] Starting GetFullDatabaseAsync - BaseURL: {BaseURL}");
        
        var uri = new Uri($"{BaseURL}/.json");
        Console.WriteLine($"[UrlService] Requesting URI: {uri}");
        
        var result = await GetStringAsync(uri);

        if (result.Error != null)
        {
            Console.WriteLine($"[UrlService] ❌ GetStringAsync failed - Error: {result.Error.ExceptionTitle} - {result.Error.Message}");
            return new Result<RootModel>(result.Error);
        }

        Console.WriteLine($"[UrlService] ✅ GetStringAsync succeeded - JSON length: {result.Value?.Length ?? 0} chars");

        try
        {
            Console.WriteLine($"[UrlService] Starting JSON deserialization...");
            Console.WriteLine($"[UrlService] First 200 chars of JSON: {result.Value?.Substring(0, Math.Min(200, result.Value.Length))}");
            
            // Use source-generated JSON deserialization (Release-safe, linker-safe, AOT-compatible)
            var model = JsonSerializer.Deserialize(result.Value!, AppJsonContext.Default.RootModel);

            if (model == null)
            {
                Console.WriteLine($"[UrlService] ❌ Deserialization returned null");
                return new Result<RootModel>(new TitledException("Error", "Failed to parse database."));
            }

            Console.WriteLine($"[UrlService] ✅ Deserialization succeeded");
            Console.WriteLine($"[UrlService] - Database Version: {model.Database?.Version ?? -1}");
            Console.WriteLine($"[UrlService] - Translations count: {model.Translations?.Count ?? 0}");
            
            return new Result<RootModel>(model);
        }
        catch (JsonException jsonEx)
        {
            Console.WriteLine($"[UrlService] ❌ JSON Deserialization Exception: {jsonEx.Message}");
            Console.WriteLine($"[UrlService] JSON Exception Type: {jsonEx.GetType().Name}");
            Console.WriteLine($"[UrlService] JSON Line/Position: {jsonEx.LineNumber}:{jsonEx.BytePositionInLine}");
            Console.WriteLine($"[UrlService] Stack Trace: {jsonEx.StackTrace}");
            return new Result<RootModel>(new TitledException("JSON Error", $"Failed to parse JSON at line {jsonEx.LineNumber}: {jsonEx.Message}"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UrlService] ❌ Deserialization Exception: {ex.Message}");
            Console.WriteLine($"[UrlService] Exception Type: {ex.GetType().Name}");
            Console.WriteLine($"[UrlService] Stack Trace: {ex.StackTrace}");
            return new Result<RootModel>(new TitledException("Error", $"Deserialization failed: {ex.Message}"));
        }
    }

    private async Task<Result<string>> GetStringAsync(Uri uri)
    {
        Console.WriteLine($"[UrlService.GetString] Starting request to: {uri}");
        
        try
        {
            // Create cancellation token with 30 second timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            
            Console.WriteLine($"[UrlService.GetString] Sending HTTP GET request...");
            var response = await _client.GetAsync(uri, cts.Token);

            Console.WriteLine($"[UrlService.GetString] Response received - Status: {response.StatusCode} ({(int)response.StatusCode})");
            Console.WriteLine($"[UrlService.GetString] IsSuccessStatusCode: {response.IsSuccessStatusCode}");
            Console.WriteLine($"[UrlService.GetString] ReasonPhrase: {response.ReasonPhrase}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[UrlService.GetString] ❌ HTTP Error - {(int)response.StatusCode} {response.ReasonPhrase}");
                return new Result<string>(new TitledException("Error", $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}"));
            }

            Console.WriteLine($"[UrlService.GetString] Reading response content...");
            var content = await response.Content.ReadAsStringAsync(cts.Token);
            
            Console.WriteLine($"[UrlService.GetString] Content read - Length: {content?.Length ?? 0} chars");
            Console.WriteLine($"[UrlService.GetString] Content is null or whitespace: {string.IsNullOrWhiteSpace(content)}");
            
            if (string.IsNullOrWhiteSpace(content))
            {
                Console.WriteLine($"[UrlService.GetString] ❌ Empty response received");
                return new Result<string>(new TitledException("Error", "Empty Response"));
            }
            
            Console.WriteLine($"[UrlService.GetString] ✅ Request successful - Returning content");
            return new Result<string>(content);
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"[UrlService.GetString] ❌ Request timed out after 30 seconds");
            return new Result<string>(new TitledException("Timeout", "Request timed out after 30 seconds. Please check your connection and try again."));
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[UrlService.GetString] ❌ HTTP Request Exception: {ex.Message}");
            Console.WriteLine($"[UrlService.GetString] Exception Type: {ex.GetType().Name}");
            Console.WriteLine($"[UrlService.GetString] Stack Trace: {ex.StackTrace}");
            return new Result<string>(new TitledException("Network Error", $"Connection failed: {ex.Message}"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UrlService.GetString] ❌ Unexpected Exception: {ex.Message}");
            Console.WriteLine($"[UrlService.GetString] Exception Type: {ex.GetType().Name}");
            Console.WriteLine($"[UrlService.GetString] Stack Trace: {ex.StackTrace}");
            return new Result<string>(new TitledException("Error", $"Request failed: {ex.Message}"));
        }
    }

    public async Task<Result<int>> GetRemoteDatabaseVersionAsync()
    {
        var uri = new Uri($"{BaseURL}/Database/Version.json");
        var result = await GetStringAsync(uri);

        if (result.Error != null)
            return new Result<int>(result.Error);

        try
        {
            if (int.TryParse(result.Value, out var version))
            {
                return new Result<int>(version);
            }

            return new Result<int>(new TitledException("Error", "Failed to parse version number."));
        }
        catch (Exception ex)
        {
            return new Result<int>(new TitledException("Error", $"Deserialization failed: {ex.Message}"));
        }
    }
}
