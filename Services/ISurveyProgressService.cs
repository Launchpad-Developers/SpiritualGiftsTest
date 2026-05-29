using SpiritualGiftsSurvey.Enums;
using SpiritualGiftsSurvey.Models;

namespace SpiritualGiftsSurvey.Services;

/// <summary>
/// Service for managing survey progress persistence.
/// Enables users to resume surveys after app restart.
/// </summary>
public interface ISurveyProgressService
{
    /// <summary>
    /// Gets the most recent active survey progress, if any.
    /// Returns null if no progress exists or if progress is invalid.
    /// </summary>
    Task<SurveyProgress?> GetActiveProgressAsync();

    /// <summary>
    /// Saves or updates survey progress.
    /// Enforces single active progress - deletes old progress before saving new.
    /// </summary>
    /// <param name="progress">Progress to save</param>
    Task SaveProgressAsync(SurveyProgress progress);

    /// <summary>
    /// Updates a single answer in the active progress.
    /// Fast update for individual answer changes.
    /// </summary>
    /// <param name="questionGuid">Question GUID</param>
    /// <param name="value">User's answer value</param>
    Task UpdateAnswerAsync(Guid questionGuid, UserValue value);

    /// <summary>
    /// Updates the current page in the active progress.
    /// </summary>
    /// <param name="page">Current page number (1-based)</param>
    Task UpdateCurrentPageAsync(int page);

    /// <summary>
    /// Clears all survey progress.
    /// Called after successful survey completion.
    /// </summary>
    Task ClearProgressAsync();

    /// <summary>
    /// Checks if there is any active progress available.
    /// </summary>
    Task<bool> HasActiveProgressAsync();
}
