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

        var top6DistinctScores = ordered
            .Select(x => x.Score)
            .Distinct()
            .Take(6)
            .ToList();

        var primaryTiers = top6DistinctScores.Take(3).ToHashSet();
        var secondaryTiers = top6DistinctScores.Skip(3).Take(3).ToHashSet();

        foreach (var score in ordered)
        {
            score.GiftRank = primaryTiers.Contains(score.Score)
                ? GiftRank.Primary
                : secondaryTiers.Contains(score.Score)
                    ? GiftRank.Secondary
                    : GiftRank.None;
        }
    }
}
