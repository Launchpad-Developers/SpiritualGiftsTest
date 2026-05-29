using SQLite;

namespace SpiritualGiftsSurvey.Models;

/// <summary>
/// Stores in-progress survey state to enable resume after app restart.
/// Only ONE active progress exists at a time - enforced by service layer.
/// </summary>
[Table("SurveyProgress")]
public class SurveyProgress
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// Unique identifier for this survey attempt/session
    /// </summary>
    public Guid SessionGuid { get; set; } = Guid.NewGuid();

    /// <summary>
    /// When the user first started this survey
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Last time progress was updated (answer changed or page navigated)
    /// </summary>
    public DateTime LastUpdatedAt { get; set; }

    /// <summary>
    /// Language code (e.g., "en", "ar") - must match to restore
    /// </summary>
    public string LanguageCode { get; set; } = string.Empty;

    /// <summary>
    /// Current page number (1-based index)
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// JSON array of Question GUIDs in shuffled order.
    /// CRITICAL: Must persist exact shuffle order - cannot re-shuffle on resume!
    /// Example: ["guid1", "guid2", "guid3", ...]
    /// </summary>
    public string QuestionOrderJson { get; set; } = string.Empty;

    /// <summary>
    /// JSON dictionary mapping Question GUID to UserValue.
    /// Example: { "guid1": 0, "guid2": 3, "guid3": 1 }
    /// UserValue: 0=NotAtAll, 1=Little, 2=Some, 3=Much, -1=DidNotAnswer
    /// </summary>
    public string AnswersJson { get; set; } = string.Empty;
}
