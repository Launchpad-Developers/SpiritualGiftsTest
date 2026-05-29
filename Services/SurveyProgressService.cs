using SpiritualGiftsSurvey.Enums;
using SpiritualGiftsSurvey.Models;
using SQLite;
using System.Text.Json;

namespace SpiritualGiftsSurvey.Services;

/// <summary>
/// Implementation of survey progress persistence service.
/// Uses SQLite for durable storage across app restarts.
/// </summary>
public class SurveyProgressService : ISurveyProgressService
{
    private readonly IDeviceStorageService _deviceStorage;

    public SurveyProgressService(IDeviceStorageService deviceStorage)
    {
        _deviceStorage = deviceStorage;
    }

    private string DatabasePath => _deviceStorage.GetDatabaseFileLocation();

    public async Task<SurveyProgress?> GetActiveProgressAsync()
    {
        try
        {
            using var conn = new SQLiteConnection(DatabasePath);
            conn.CreateTable<SurveyProgress>();

            // Get most recent progress (there should only be one, but take latest just in case)
            var progress = conn.Table<SurveyProgress>()
                .OrderByDescending(p => p.LastUpdatedAt)
                .FirstOrDefault();

            return await Task.FromResult(progress);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SurveyProgressService] Error getting active progress: {ex.Message}");
            return null;
        }
    }

    public async Task SaveProgressAsync(SurveyProgress progress)
    {
        try
        {
            using var conn = new SQLiteConnection(DatabasePath);
            conn.CreateTable<SurveyProgress>();

            // Enforce single active progress - delete any existing progress
            conn.DeleteAll<SurveyProgress>();

            // Update timestamp
            progress.LastUpdatedAt = DateTime.UtcNow;

            // Insert new progress
            conn.Insert(progress);

            Console.WriteLine($"[SurveyProgressService] Progress saved - Session: {progress.SessionGuid}, Page: {progress.CurrentPage}");

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SurveyProgressService] Error saving progress: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateAnswerAsync(Guid questionGuid, UserValue value)
    {
        try
        {
            using var conn = new SQLiteConnection(DatabasePath);
            conn.CreateTable<SurveyProgress>();

            var progress = conn.Table<SurveyProgress>()
                .OrderByDescending(p => p.LastUpdatedAt)
                .FirstOrDefault();

            if (progress == null)
            {
                Console.WriteLine($"[SurveyProgressService] No active progress to update answer");
                return;
            }

            // Deserialize current answers
            var answers = string.IsNullOrEmpty(progress.AnswersJson)
                ? new Dictionary<string, int>()
                : JsonSerializer.Deserialize<Dictionary<string, int>>(progress.AnswersJson) ?? new Dictionary<string, int>();

            // Update answer
            answers[questionGuid.ToString()] = (int)value;

            // Serialize back
            progress.AnswersJson = JsonSerializer.Serialize(answers);
            progress.LastUpdatedAt = DateTime.UtcNow;

            // Update database
            conn.Update(progress);

            Console.WriteLine($"[SurveyProgressService] Answer updated - Question: {questionGuid}, Value: {value}");

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SurveyProgressService] Error updating answer: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateCurrentPageAsync(int page)
    {
        try
        {
            using var conn = new SQLiteConnection(DatabasePath);
            conn.CreateTable<SurveyProgress>();

            var progress = conn.Table<SurveyProgress>()
                .OrderByDescending(p => p.LastUpdatedAt)
                .FirstOrDefault();

            if (progress == null)
            {
                Console.WriteLine($"[SurveyProgressService] No active progress to update page");
                return;
            }

            progress.CurrentPage = page;
            progress.LastUpdatedAt = DateTime.UtcNow;

            conn.Update(progress);

            Console.WriteLine($"[SurveyProgressService] Current page updated - Page: {page}");

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SurveyProgressService] Error updating page: {ex.Message}");
            throw;
        }
    }

    public async Task ClearProgressAsync()
    {
        try
        {
            using var conn = new SQLiteConnection(DatabasePath);
            conn.CreateTable<SurveyProgress>();

            conn.DeleteAll<SurveyProgress>();

            Console.WriteLine($"[SurveyProgressService] All progress cleared");

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SurveyProgressService] Error clearing progress: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> HasActiveProgressAsync()
    {
        var progress = await GetActiveProgressAsync();
        return progress != null;
    }
}
