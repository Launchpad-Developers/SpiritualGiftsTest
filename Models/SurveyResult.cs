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

    // Optional full raw results snapshot
    public string Results { get; set; } = string.Empty;

    [Ignore]
    public List<UserGiftScore> Scores { get; set; } = new();

    public void RankGifts()
    {
        if (Scores == null || !Scores.Any())
            return;

        var ordered = Scores.OrderByDescending(x => x.Score).ToList();
        var scoreGroups = ordered
            .GroupBy(x => x.Score)
            .OrderByDescending(g => g.Key)
            .ToList();

        var primary = new List<UserGiftScore>();
        var secondary = new List<UserGiftScore>();

        int totalSlots = 0;

        // Always include the top score group as Primary
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

        // Add Secondary only if fewer than 6 total
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
                // Allow tie overflow if it would go over 6
                secondary.AddRange(group);
                break;
            }
        }

        foreach (var score in ordered)
        {
            if (primary.Contains(score))
                score.GiftRank = GiftRank.Primary;
            else if (secondary.Contains(score))
                score.GiftRank = GiftRank.Secondary;
            else
                score.GiftRank = GiftRank.None;
        }
    }
}
