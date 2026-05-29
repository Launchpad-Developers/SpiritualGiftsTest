using SpiritualGiftsSurvey.Enums;
using SpiritualGiftsSurvey.Models;

namespace SpiritualGiftsSurvey.Services;

/// <summary>
/// Test stub for ISurveyProgressService to avoid MAUI dependencies in test project.
/// The actual implementation is in the main project.
/// </summary>
public interface ISurveyProgressService
{
    Task<SurveyProgress?> GetActiveProgressAsync();
    Task SaveProgressAsync(SurveyProgress progress);
    Task UpdateAnswerAsync(Guid questionGuid, UserValue value);
    Task UpdateCurrentPageAsync(int page);
    Task ClearProgressAsync();
    Task<bool> HasActiveProgressAsync();
}
