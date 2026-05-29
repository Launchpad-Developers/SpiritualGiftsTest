using SpiritualGiftsSurvey.Enums;
using SQLite;

namespace SpiritualGiftsSurvey.Models;

[Table("SurveyResults")]
public class SurveyResult
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public Guid UserGiftResultGuid { get; set; } = Guid.NewGuid();

    public DateTime DateTaken { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Results { get; set; } = string.Empty;

    [Ignore]
    public List<UserGiftScore> Scores { get; set; } = new();

    [Ignore]
    public bool IsRanked { get; private set; }

    // HIGH-5 FIX: Concurrent ranking guard
    private Task? _rankingTask;
    private readonly SemaphoreSlim _rankLock = new(1, 1);

    /// <summary>
    /// HIGH-5 FIX: Thread-safe ranking with concurrency protection.
    /// Prevents overlapping ranking execution and unsafe background thread mutation.
    /// </summary>
    public async Task RankGiftsAsync()
    {
        if (Scores == null || !Scores.Any() || IsRanked)
            return;

        // If ranking is already in progress, await the existing task
        if (_rankingTask != null && !_rankingTask.IsCompleted)
        {
            await _rankingTask;
            return;
        }

        // Acquire lock to prevent concurrent ranking
        await _rankLock.WaitAsync();
        try
        {
            // Double-check: another caller might have completed ranking while we waited
            if (IsRanked)
                return;

            if (_rankingTask != null && !_rankingTask.IsCompleted)
            {
                await _rankingTask;
                return;
            }

            // Perform ranking computation
            _rankingTask = Task.Run(() =>
            {
                var ordered = Scores.OrderByDescending(x => x.Score).ToList();
                var scoreGroups = ordered
                    .GroupBy(x => x.Score)
                    .OrderByDescending(g => g.Key)
                    .ToList();

                var primary = new List<UserGiftScore>();
                var secondary = new List<UserGiftScore>();
                int totalSlots = 0;

                foreach (var group in scoreGroups)
                {
                    if (primary.Count == 0)
                    {
                        primary.AddRange(group);
                        totalSlots = primary.Count;
                    }
                    else if (primary.Count < 3)
                    {
                        primary.AddRange(group);
                        totalSlots = primary.Count;
                    }
                    else
                    {
                        break;
                    }
                }

                foreach (var group in scoreGroups.SkipWhile(g => primary.Contains(g.First())))
                {
                    if (totalSlots >= 6)
                        break;

                    if (totalSlots + group.Count() <= 6)
                    {
                        secondary.AddRange(group);
                        totalSlots += group.Count();
                    }
                    else
                    {
                        secondary.AddRange(group);
                        break;
                    }
                }

                // HIGH-5 FIX: Mutation still happens on background thread,
                // but we hold the lock so concurrent calls are serialized
                foreach (var score in ordered)
                {
                    if (primary.Contains(score))
                        score.GiftRank = GiftRank.Primary;
                    else if (secondary.Contains(score))
                        score.GiftRank = GiftRank.Secondary;
                    else
                        score.GiftRank = GiftRank.None;
                }
            });

            await _rankingTask;
            IsRanked = true;
        }
        finally
        {
            _rankLock.Release();
        }
    }
}
