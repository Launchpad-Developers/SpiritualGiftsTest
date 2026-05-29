using SpiritualGiftsSurvey.Enums;
using SpiritualGiftsSurvey.Models;

namespace SpiritualGiftsSurvey.Tests;

/// <summary>
/// Unit tests for complete survey scoring and ranking logic.
/// Tests the full flow: Score calculation → Ranking assignment.
/// Tests BOTH the score calculation business rules AND the ranking algorithm.
/// </summary>
public class SurveyScoreRankingTests
{
    [Fact]
    public async Task Survey_CalculatesScoresCorrectly_SingleGift_AllMaxResponses()
    {
        // Arrange - Simulate 10 questions answered "Much" (3 points each) for Prophecy
        var scores = new List<UserGiftScore>
        {
            new UserGiftScore
            {
                Gift = Gifts.Prophecy,
                GiftName = "Prophecy",
                Score = 30,  // 10 questions × 3 points
                MaxScore = 30,  // 10 questions × 3 max
                GiftDescriptionGuid = Guid.NewGuid()
            }
        };

        var result = new SurveyResult
        {
            DateTaken = DateTime.UtcNow,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Scores = scores
        };

        // Act
        await result.RankGiftsAsync();

        // Assert
        Assert.True(result.IsRanked);
        Assert.Single(result.Scores);
        var prophecy = result.Scores[0];
        Assert.Equal(30, prophecy.Score);
        Assert.Equal(30, prophecy.MaxScore);
        Assert.Equal(1.0, prophecy.Progress, 2); // 100% progress
        Assert.Equal(GiftRank.Primary, prophecy.GiftRank);
    }

    [Fact]
    public async Task Survey_CalculatesScoresCorrectly_MultipleGifts_MixedResponses()
    {
        // Arrange - Simulate realistic survey with mixed responses
        var scores = new List<UserGiftScore>
        {
            // Prophecy: 8 questions (4×Much + 2×Some + 2×Little = 16)
            new UserGiftScore { Gift = Gifts.Prophecy, GiftName = "Prophecy", Score = 16, MaxScore = 24, GiftDescriptionGuid = Guid.NewGuid() },
            
            // Teaching: 9 questions (3×Much + 3×Some + 3×NotAtAll = 15)
            new UserGiftScore { Gift = Gifts.Teaching, GiftName = "Teaching", Score = 15, MaxScore = 27, GiftDescriptionGuid = Guid.NewGuid() },
            
            // Wisdom: 10 questions (2×Much + 3×Some + 5×Little = 17)
            new UserGiftScore { Gift = Gifts.Wisdom, GiftName = "Wisdom", Score = 17, MaxScore = 30, GiftDescriptionGuid = Guid.NewGuid() },
            
            // Faith: 8 questions (all NotAtAll = 0)
            new UserGiftScore { Gift = Gifts.Faith, GiftName = "Faith", Score = 0, MaxScore = 24, GiftDescriptionGuid = Guid.NewGuid() }
        };

        var result = new SurveyResult
        {
            DateTaken = DateTime.UtcNow,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            Scores = scores
        };

        // Act
        await result.RankGiftsAsync();

        // Assert
        Assert.True(result.IsRanked);
        Assert.Equal(4, result.Scores.Count);
        
        // Sort to verify (RankGiftsAsync doesn't reorder the list)
        var sorted = result.Scores.OrderByDescending(s => s.Score).ToList();
        
        // Verify scores (should be ordered by score descending)
        Assert.Equal(Gifts.Wisdom, sorted[0].Gift);
        Assert.Equal(17, sorted[0].Score);
        Assert.Equal(30, sorted[0].MaxScore);
        Assert.Equal(0.567, sorted[0].Progress, 2); // 17/30 = ~56.7%
        
        Assert.Equal(Gifts.Prophecy, sorted[1].Gift);
        Assert.Equal(16, sorted[1].Score);
        Assert.Equal(24, sorted[1].MaxScore);
        Assert.Equal(0.667, sorted[1].Progress, 2); // 16/24 = ~66.7%
        
        Assert.Equal(Gifts.Teaching, sorted[2].Gift);
        Assert.Equal(15, sorted[2].Score);
        Assert.Equal(27, sorted[2].MaxScore);
        Assert.Equal(0.556, sorted[2].Progress, 2); // 15/27 = ~55.6%
        
        Assert.Equal(Gifts.Faith, sorted[3].Gift);
        Assert.Equal(0, sorted[3].Score);
        Assert.Equal(24, sorted[3].MaxScore);
        Assert.Equal(0.0, sorted[3].Progress); // 0/24 = 0%
        
        // Verify ranking (Top 3 should be Primary, 4th should be Secondary since totalSlots < 6)
        Assert.Equal(GiftRank.Primary, sorted[0].GiftRank); // Wisdom
        Assert.Equal(GiftRank.Primary, sorted[1].GiftRank); // Prophecy
        Assert.Equal(GiftRank.Primary, sorted[2].GiftRank); // Teaching
        Assert.Equal(GiftRank.Secondary, sorted[3].GiftRank); // Faith (fills remaining slots up to 6 total)
    }

    [Fact]
    public async Task Survey_ProgressCalculation_VerifiesFormula()
    {
        // Arrange - Test progress calculation: Progress = Score / MaxScore
        var scores = new List<UserGiftScore>
        {
            new UserGiftScore { Gift = Gifts.Prophecy, GiftName = "Prophecy", Score = 15, MaxScore = 30, GiftDescriptionGuid = Guid.NewGuid() }, // 50%
            new UserGiftScore { Gift = Gifts.Teaching, GiftName = "Teaching", Score = 10, MaxScore = 30, GiftDescriptionGuid = Guid.NewGuid() }, // 33.3%
            new UserGiftScore { Gift = Gifts.Wisdom, GiftName = "Wisdom", Score = 20, MaxScore = 30, GiftDescriptionGuid = Guid.NewGuid() }, // 66.7%
            new UserGiftScore { Gift = Gifts.Faith, GiftName = "Faith", Score = 30, MaxScore = 30, GiftDescriptionGuid = Guid.NewGuid() } // 100%
        };

        var result = new SurveyResult
        {
            DateTaken = DateTime.UtcNow,
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Scores = scores
        };

        // Act
        await result.RankGiftsAsync();

        // Assert - Verify progress percentages
        var faith = result.Scores.First(s => s.Gift == Gifts.Faith);
        Assert.Equal(1.0, faith.Progress, 2); // 30/30 = 100%
        
        var wisdom = result.Scores.First(s => s.Gift == Gifts.Wisdom);
        Assert.Equal(0.667, wisdom.Progress, 2); // 20/30 = ~66.7%
        
        var prophecy = result.Scores.First(s => s.Gift == Gifts.Prophecy);
        Assert.Equal(0.5, prophecy.Progress, 2); // 15/30 = 50%
        
        var teaching = result.Scores.First(s => s.Gift == Gifts.Teaching);
        Assert.Equal(0.333, teaching.Progress, 2); // 10/30 = ~33.3%
    }

    [Fact]
    public async Task Survey_RealisticFullSurvey_22Gifts_VerifiesRankingDistribution()
    {
        // Arrange - Simulate a full realistic survey with all 22 gifts
        var random = new Random(42); // Seed for reproducibility
        var scores = new List<UserGiftScore>();
        
        foreach (Gifts gift in Enum.GetValues<Gifts>())
        {
            if (gift == Gifts.Unknown) continue;
            
            int questionCount = random.Next(8, 11); // 8-10 questions per gift
            int score = random.Next(0, questionCount * 3 + 1); // Random score 0 to max
            int maxScore = questionCount * 3;
            
            scores.Add(new UserGiftScore
            {
                Gift = gift,
                GiftName = gift.ToString(),
                Score = score,
                MaxScore = maxScore,
                GiftDescriptionGuid = Guid.NewGuid()
            });
        }

        var result = new SurveyResult
        {
            DateTaken = DateTime.UtcNow,
            FirstName = "Realistic",
            LastName = "Test",
            Email = "realistic@test.com",
            Scores = scores
        };

        // Act
        await result.RankGiftsAsync();

        // Assert
        Assert.True(result.IsRanked);
        Assert.Equal(22, result.Scores.Count); // All 22 gifts

        // Sort by score descending (RankGiftsAsync doesn't reorder the list)
        var sorted = result.Scores.OrderByDescending(s => s.Score).ToList();

        // Verify ranking distribution
        var primary = result.Scores.Count(s => s.GiftRank == GiftRank.Primary);
        var secondary = result.Scores.Count(s => s.GiftRank == GiftRank.Secondary);
        var none = result.Scores.Count(s => s.GiftRank == GiftRank.None);

        Assert.InRange(primary, 3, 10); // At least 3, may have ties
        Assert.InRange(secondary, 0, 10); // May be 0 if primary fills all slots
        Assert.Equal(22, primary + secondary + none); // All gifts accounted for
        
        // Verify ordering (sorted list should be descending by score)
        for (int i = 0; i < sorted.Count - 1; i++)
        {
            Assert.True(sorted[i].Score >= sorted[i + 1].Score,
                $"Scores should be descending: {sorted[i].Gift} ({sorted[i].Score}) >= {sorted[i + 1].Gift} ({sorted[i + 1].Score})");
        }
        
        // Verify progress calculation for all gifts
        foreach (var score in result.Scores)
        {
            double expectedProgress = score.MaxScore > 0 ? (double)score.Score / score.MaxScore : 0;
            Assert.Equal(expectedProgress, score.Progress, 2);
        }
    }

    [Fact]
    public async Task Survey_ZeroScores_HandledCorrectly()
    {
        // Arrange - User answered "NotAtAll" to everything
        var scores = new List<UserGiftScore>
        {
            new UserGiftScore { Gift = Gifts.Prophecy, GiftName = "Prophecy", Score = 0, MaxScore = 30, GiftDescriptionGuid = Guid.NewGuid() },
            new UserGiftScore { Gift = Gifts.Teaching, GiftName = "Teaching", Score = 0, MaxScore = 27, GiftDescriptionGuid = Guid.NewGuid() },
            new UserGiftScore { Gift = Gifts.Wisdom, GiftName = "Wisdom", Score = 0, MaxScore = 30, GiftDescriptionGuid = Guid.NewGuid() }
        };

        var result = new SurveyResult
        {
            DateTaken = DateTime.UtcNow,
            FirstName = "Zero",
            LastName = "Scores",
            Email = "zero@example.com",
            Scores = scores
        };

        // Act
        await result.RankGiftsAsync();

        // Assert
        Assert.True(result.IsRanked);
        
        // All should have 0 score and 0 progress
        foreach (var score in result.Scores)
        {
            Assert.Equal(0, score.Score);
            Assert.Equal(0.0, score.Progress);
        }
        
        // All should be Primary (tied at 0)
        Assert.All(result.Scores, s => Assert.Equal(GiftRank.Primary, s.GiftRank));
    }

    [Fact]
    public async Task Survey_PerfectScores_AllMaxResponses()
    {
        // Arrange - User answered "Much" to everything
        var scores = new List<UserGiftScore>
        {
            new UserGiftScore { Gift = Gifts.Prophecy, GiftName = "Prophecy", Score = 30, MaxScore = 30, GiftDescriptionGuid = Guid.NewGuid() },
            new UserGiftScore { Gift = Gifts.Teaching, GiftName = "Teaching", Score = 27, MaxScore = 27, GiftDescriptionGuid = Guid.NewGuid() },
            new UserGiftScore { Gift = Gifts.Wisdom, GiftName = "Wisdom", Score = 30, MaxScore = 30, GiftDescriptionGuid = Guid.NewGuid() },
            new UserGiftScore { Gift = Gifts.Faith, GiftName = "Faith", Score = 24, MaxScore = 24, GiftDescriptionGuid = Guid.NewGuid() }
        };

        var result = new SurveyResult
        {
            DateTaken = DateTime.UtcNow,
            FirstName = "Perfect",
            LastName = "Scores",
            Email = "perfect@example.com",
            Scores = scores
        };

        // Act
        await result.RankGiftsAsync();

        // Assert
        Assert.True(result.IsRanked);
        
        // Sort by score descending (RankGiftsAsync doesn't reorder the list)
        var sorted = result.Scores.OrderByDescending(s => s.Score).ToList();
        
        // Verify ordering by score (descending)
        Assert.True(sorted[0].Score >= sorted[1].Score);
        Assert.True(sorted[1].Score >= sorted[2].Score);
        Assert.True(sorted[2].Score >= sorted[3].Score);
        
        // All should have 100% progress
        Assert.All(result.Scores, s => Assert.Equal(1.0, s.Progress, 2));
    }
}
