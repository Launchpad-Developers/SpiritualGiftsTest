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
        var uri = new Uri($"{BaseURL}/.json");
        var result = await GetStringAsync(uri);

        if (result.Error != null)
            return new Result<RootModel>(result.Error);

        try
        {
            var model = JsonSerializer.Deserialize<RootModel>(result.Value!, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return model != null
                ? new Result<RootModel>(model)
                : new Result<RootModel>(new TitledException("Error", "Failed to parse database."));
        }
        catch (Exception ex)
        {
            return new Result<RootModel>(new TitledException("Error", $"Deserialization failed: {ex.Message}"));
        }
    }

    private async Task<Result<string>> GetStringAsync(Uri uri)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return new Result<string>(new TitledException("Error", "No Internet Connection"));

        try
        {
            var response = await _client.GetAsync(uri);

            if (!response.IsSuccessStatusCode)
                return new Result<string>(new TitledException("Error", $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}"));

            var content = await response.Content.ReadAsStringAsync();
            return string.IsNullOrWhiteSpace(content)
                ? new Result<string>(new TitledException("Error", "Empty Response"))
                : new Result<string>(content);
        }
        catch (Exception ex)
        {
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
