using SpiritualGiftsSurvey.Enums;
using SpiritualGiftsSurvey.Models;
using FsCheck;
using FsCheck.Xunit;

namespace SpiritualGiftsSurvey.Tests;

/// <summary>
/// Regression tests for SurveyResult.RankGiftsAsync() stabilization work (HIGH-5).
/// Validates ranking logic correctness, concurrency safety, and idempotency.
/// </summary>
public class SurveyResultRankingTests
{
    [Fact]
    public async Task RankGiftsAsync_AssignsPrimaryGifts_ForTopScores()
    {
        // Arrange
        var result = new SurveyResult
        {
            Scores = new List<UserGiftScore>
            {
                new() { Gift = Gifts.Prophecy, Score = 50 },
                new() { Gift = Gifts.Teaching, Score = 48 },
                new() { Gift = Gifts.Wisdom, Score = 45 },
                new() { Gift = Gifts.Faith, Score = 30 },
                new() { Gift = Gifts.Mercy, Score = 25 }
            }
        };

        // Act
        await result.RankGiftsAsync();

        // Assert
        var primary = result.Scores.Where(s => s.GiftRank == GiftRank.Primary).ToList();
        Assert.Equal(3, primary.Count);
        Assert.Contains(primary, s => s.Gift == Gifts.Prophecy);
        Assert.Contains(primary, s => s.Gift == Gifts.Teaching);
        Assert.Contains(primary, s => s.Gift == Gifts.Wisdom);
        Assert.True(result.IsRanked);
    }

    [Fact]
    public async Task RankGiftsAsync_AssignsSecondaryGifts_ForMiddleScores()
    {
        // Arrange
        var result = new SurveyResult
        {
            Scores = new List<UserGiftScore>
            {
                new() { Gift = Gifts.Prophecy, Score = 50 },
                new() { Gift = Gifts.Teaching, Score = 48 },
                new() { Gift = Gifts.Wisdom, Score = 45 },
                new() { Gift = Gifts.Faith, Score = 30 },
                new() { Gift = Gifts.Mercy, Score = 28 },
                new() { Gift = Gifts.Giving, Score = 26 },
                new() { Gift = Gifts.Leadership, Score = 20 }
            }
        };

        // Act
        await result.RankGiftsAsync();

        // Assert
        var secondary = result.Scores.Where(s => s.GiftRank == GiftRank.Secondary).ToList();
        Assert.InRange(secondary.Count, 1, 3); // Secondary can be 1-3 gifts
        Assert.True(result.IsRanked);
    }

    [Fact]
    public async Task RankGiftsAsync_HandlesTies_IncludesAllTiedGiftsInSameRank()
    {
        // Arrange - Three gifts tied at top score
        var result = new SurveyResult
        {
            Scores = new List<UserGiftScore>
            {
                new() { Gift = Gifts.Prophecy, Score = 50 },
                new() { Gift = Gifts.Teaching, Score = 50 },
                new() { Gift = Gifts.Wisdom, Score = 50 },
                new() { Gift = Gifts.Faith, Score = 30 },
                new() { Gift = Gifts.Mercy, Score = 25 }
            }
        };

        // Act
        await result.RankGiftsAsync();

        // Assert
        var primary = result.Scores.Where(s => s.GiftRank == GiftRank.Primary).ToList();
        Assert.Equal(3, primary.Count); // All tied scores should be in primary
        Assert.Contains(primary, s => s.Gift == Gifts.Prophecy);
        Assert.Contains(primary, s => s.Gift == Gifts.Teaching);
        Assert.Contains(primary, s => s.Gift == Gifts.Wisdom);
        Assert.True(result.IsRanked);
    }

    [Fact]
    public async Task RankGiftsAsync_DoesNotCrash_WhenScoresIsEmpty()
    {
        // Arrange
        var result = new SurveyResult { Scores = new List<UserGiftScore>() };

        // Act
        await result.RankGiftsAsync();

        // Assert
        Assert.False(result.IsRanked); // Should not rank empty collection
    }

    [Fact]
    public async Task RankGiftsAsync_DoesNotCrash_WhenScoresIsNull()
    {
        // Arrange
        var result = new SurveyResult { Scores = null! };

        // Act
        await result.RankGiftsAsync();

        // Assert
        Assert.False(result.IsRanked); // Should not rank null collection
    }

    [Fact]
    public async Task RankGiftsAsync_IsIdempotent_MultipleCalls()
    {
        // Arrange
        var result = new SurveyResult
        {
            Scores = new List<UserGiftScore>
            {
                new() { Gift = Gifts.Prophecy, Score = 50 },
                new() { Gift = Gifts.Teaching, Score = 48 },
                new() { Gift = Gifts.Wisdom, Score = 45 }
            }
        };

        // Act
        await result.RankGiftsAsync();
        var firstRanking = result.Scores.Select(s => new { s.Gift, s.GiftRank }).ToList();

        await result.RankGiftsAsync();
        var secondRanking = result.Scores.Select(s => new { s.Gift, s.GiftRank }).ToList();

        // Assert
        Assert.Equal(firstRanking.Count, secondRanking.Count);
        for (int i = 0; i < firstRanking.Count; i++)
        {
            Assert.Equal(firstRanking[i].Gift, secondRanking[i].Gift);
            Assert.Equal(firstRanking[i].GiftRank, secondRanking[i].GiftRank);
        }
    }

    [Fact]
    public async Task RankGiftsAsync_ConcurrentCalls_DoNotCorruptState()
    {
        // Arrange
        var result = new SurveyResult
        {
            Scores = new List<UserGiftScore>
            {
                new() { Gift = Gifts.Prophecy, Score = 50 },
                new() { Gift = Gifts.Teaching, Score = 48 },
                new() { Gift = Gifts.Wisdom, Score = 45 },
                new() { Gift = Gifts.Faith, Score = 30 },
                new() { Gift = Gifts.Mercy, Score = 25 }
            }
        };

        // Act - Fire multiple concurrent ranking calls (HIGH-5 fix should serialize these)
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => result.RankGiftsAsync())
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert - All scores should have consistent rankings
        Assert.True(result.IsRanked);
        var primary = result.Scores.Where(s => s.GiftRank == GiftRank.Primary).ToList();
        Assert.Equal(3, primary.Count);
        
        // Verify no corruption: every score should have a rank assigned
        Assert.All(result.Scores, score =>
        {
            Assert.True(score.GiftRank == GiftRank.Primary || score.GiftRank == GiftRank.Secondary || score.GiftRank == GiftRank.None);
        });
    }

    [Fact]
    public async Task RankGiftsAsync_AssignsNoRank_ForLowestScores()
    {
        // Arrange
        var result = new SurveyResult
        {
            Scores = new List<UserGiftScore>
            {
                new() { Gift = Gifts.Prophecy, Score = 50 },
                new() { Gift = Gifts.Teaching, Score = 48 },
                new() { Gift = Gifts.Wisdom, Score = 45 },
                new() { Gift = Gifts.Faith, Score = 30 },
                new() { Gift = Gifts.Mercy, Score = 28 },
                new() { Gift = Gifts.Giving, Score = 26 },
                new() { Gift = Gifts.Leadership, Score = 20 },
                new() { Gift = Gifts.Hospitality, Score = 15 }, // Should get None
                new() { Gift = Gifts.Service, Score = 10 } // Should get None
            }
        };

        // Act
        await result.RankGiftsAsync();

        // Assert
        var noneRank = result.Scores.Where(s => s.GiftRank == GiftRank.None).ToList();
        Assert.NotEmpty(noneRank); // Some scores should get None rank
        Assert.Contains(noneRank, s => s.Gift == Gifts.Hospitality || s.Gift == Gifts.Service);
    }

    // ============================================================================
    // EDGE CASE TESTS - Added 2026-05-29
    // Tests for previously untested scenarios identified in business logic review
    // ============================================================================

    [Fact]
    public async Task RankGiftsAsync_SingleGift_AssignsPrimary()
    {
        // Arrange - Only one gift in the entire survey
        var result = new SurveyResult
        {
            Scores = new List<UserGiftScore>
            {
                new() { Gift = Gifts.Prophecy, Score = 50 }
            }
        };

        // Act
        await result.RankGiftsAsync();

        // Assert
        Assert.True(result.IsRanked);
        var primary = result.Scores.Where(s => s.GiftRank == GiftRank.Primary).ToList();
        Assert.Single(primary); // Only one gift, should be Primary
        Assert.Equal(Gifts.Prophecy, primary[0].Gift);
        
        var secondary = result.Scores.Where(s => s.GiftRank == GiftRank.Secondary).ToList();
        Assert.Empty(secondary); // No secondary gifts
        
        var none = result.Scores.Where(s => s.GiftRank == GiftRank.None).ToList();
        Assert.Empty(none); // No unranked gifts
    }

    [Fact]
    public async Task RankGiftsAsync_TwoGifts_AssignsBothAsPrimary()
    {
        // Arrange - Only two gifts total (less than minimum 3 primary)
        var result = new SurveyResult
        {
            Scores = new List<UserGiftScore>
            {
                new() { Gift = Gifts.Prophecy, Score = 50 },
                new() { Gift = Gifts.Teaching, Score = 45 }
            }
        };

        // Act
        await result.RankGiftsAsync();

        // Assert
        Assert.True(result.IsRanked);
        var primary = result.Scores.Where(s => s.GiftRank == GiftRank.Primary).ToList();
        Assert.Equal(2, primary.Count); // Both gifts should be Primary
        Assert.Contains(primary, s => s.Gift == Gifts.Prophecy);
        Assert.Contains(primary, s => s.Gift == Gifts.Teaching);
        
        var secondary = result.Scores.Where(s => s.GiftRank == GiftRank.Secondary).ToList();
        Assert.Empty(secondary); // No secondary gifts
    }

    [Fact]
    public async Task RankGiftsAsync_TieCreatesExactly6Primary_AssignsNoSecondary()
    {
        // Arrange - First score group has 6 tied gifts (all get Primary, then algorithm stops)
        // Business rule: Primary assignment stops when count >= 3, not when count == 6
        // So a 6-way tie in first group = 6 Primary, algorithm stops, no Secondary evaluated
        var result = new SurveyResult
        {
            Scores = new List<UserGiftScore>
            {
                new() { Gift = Gifts.Prophecy, Score = 50 },
                new() { Gift = Gifts.Teaching, Score = 50 },
                new() { Gift = Gifts.Wisdom, Score = 50 },
                new() { Gift = Gifts.Faith, Score = 50 },
                new() { Gift = Gifts.Mercy, Score = 50 },
                new() { Gift = Gifts.Giving, Score = 50 },
                new() { Gift = Gifts.Leadership, Score = 30 },
                new() { Gift = Gifts.Service, Score = 25 }
            }
        };

        // Act
        await result.RankGiftsAsync();

        // Assert
        Assert.True(result.IsRanked);
        var primary = result.Scores.Where(s => s.GiftRank == GiftRank.Primary).ToList();
        Assert.Equal(6, primary.Count); // All 6 tied at top score get Primary
        
        var secondary = result.Scores.Where(s => s.GiftRank == GiftRank.Secondary).ToList();
        Assert.Empty(secondary); // Algorithm stopped after Primary, no Secondary assigned
        
        var none = result.Scores.Where(s => s.GiftRank == GiftRank.None).ToList();
        Assert.Equal(2, none.Count); // Remaining gifts get None
    }

    [Fact]
    public async Task RankGiftsAsync_AllGiftsSameScore_AssignsAllPrimary()
    {
        // Arrange - All gifts have identical scores (extreme tie scenario)
        var result = new SurveyResult
        {
            Scores = new List<UserGiftScore>
            {
                new() { Gift = Gifts.Prophecy, Score = 30 },
                new() { Gift = Gifts.Teaching, Score = 30 },
                new() { Gift = Gifts.Wisdom, Score = 30 },
                new() { Gift = Gifts.Faith, Score = 30 },
                new() { Gift = Gifts.Mercy, Score = 30 },
                new() { Gift = Gifts.Giving, Score = 30 }
            }
        };

        // Act
        await result.RankGiftsAsync();

        // Assert
        Assert.True(result.IsRanked);
        var primary = result.Scores.Where(s => s.GiftRank == GiftRank.Primary).ToList();
        Assert.Equal(6, primary.Count); // All tied scores should be in same rank
        
        var secondary = result.Scores.Where(s => s.GiftRank == GiftRank.Secondary).ToList();
        Assert.Empty(secondary); // No secondary - all are primary due to tie
        
        var none = result.Scores.Where(s => s.GiftRank == GiftRank.None).ToList();
        Assert.Empty(none); // No unranked - all are primary
    }

    [Fact]
    public async Task RankGiftsAsync_SecondaryTieExceedsTotalOf10_IncludesAllTied()
    {
        // Arrange - Primary has 3, Secondary boundary has 5 tied (total would be 8)
        // Business rule: Include ALL tied scores even if it exceeds target total of 6
        var result = new SurveyResult
        {
            Scores = new List<UserGiftScore>
            {
                new() { Gift = Gifts.Prophecy, Score = 50 },
                new() { Gift = Gifts.Teaching, Score = 48 },
                new() { Gift = Gifts.Wisdom, Score = 46 },
                // Secondary tier - all tied at 30
                new() { Gift = Gifts.Faith, Score = 30 },
                new() { Gift = Gifts.Mercy, Score = 30 },
                new() { Gift = Gifts.Giving, Score = 30 },
                new() { Gift = Gifts.Leadership, Score = 30 },
                new() { Gift = Gifts.Service, Score = 30 },
                // Lower tier
                new() { Gift = Gifts.Hospitality, Score = 20 }
            }
        };

        // Act
        await result.RankGiftsAsync();

        // Assert
        Assert.True(result.IsRanked);
        var primary = result.Scores.Where(s => s.GiftRank == GiftRank.Primary).ToList();
        Assert.Equal(3, primary.Count);
        
        var secondary = result.Scores.Where(s => s.GiftRank == GiftRank.Secondary).ToList();
        Assert.Equal(5, secondary.Count); // All 5 tied scores included
        
        // Total medals exceed 6 due to tie
        var totalMedals = primary.Count + secondary.Count;
        Assert.Equal(8, totalMedals);
        
        // All tied gifts at score 30 should have Secondary rank
        var tiedGifts = result.Scores.Where(s => s.Score == 30).ToList();
        Assert.All(tiedGifts, gift => Assert.Equal(GiftRank.Secondary, gift.GiftRank));
    }

    [Fact]
    public async Task RankGiftsAsync_FewDistinctScores_RanksCorrectly()
    {
        // Arrange - Only 2 distinct score values across many gifts
        var result = new SurveyResult
        {
            Scores = new List<UserGiftScore>
            {
                new() { Gift = Gifts.Prophecy, Score = 40 },
                new() { Gift = Gifts.Teaching, Score = 40 },
                new() { Gift = Gifts.Wisdom, Score = 40 },
                new() { Gift = Gifts.Faith, Score = 40 },
                new() { Gift = Gifts.Mercy, Score = 20 },
                new() { Gift = Gifts.Giving, Score = 20 },
                new() { Gift = Gifts.Leadership, Score = 20 }
            }
        };

        // Act
        await result.RankGiftsAsync();

        // Assert
        Assert.True(result.IsRanked);
        var primary = result.Scores.Where(s => s.GiftRank == GiftRank.Primary).ToList();
        Assert.Equal(4, primary.Count); // All score=40 gifts get Primary
        
        var secondary = result.Scores.Where(s => s.GiftRank == GiftRank.Secondary).ToList();
        Assert.Equal(3, secondary.Count); // All score=20 gifts get Secondary (within 6 total slots)
        
        var none = result.Scores.Where(s => s.GiftRank == GiftRank.None).ToList();
        Assert.Empty(none); // All gifts fit within medal tiers
    }

    // ============================================================================
    // PROPERTY-BASED TESTS - Added 2026-05-29
    // Validates ranking algorithm invariants across randomized score distributions
    // ============================================================================

    [Fact]
    public void RankGiftsAsync_PropertyTests_PrimaryCountInvariant()
    {
        Prop.ForAll(
            GiftScoreGenerators.ArbitraryScores(),
            validScores =>
            {
                if (validScores == null || validScores.Count == 0)
                    return true;
                
                var result = new SurveyResult { Scores = validScores };
                result.RankGiftsAsync().Wait();
                
                var primary = result.Scores.Where(s => s.GiftRank == GiftRank.Primary).ToList();
                
                // Invariant: Primary count should be at least 3, OR equal to total gift count (if < 3 total)
                if (validScores.Count < 3)
                    return primary.Count == validScores.Count;
                else
                    return primary.Count >= 3;
            }
        ).QuickCheckThrowOnFailure();
    }

    [Fact]
    public void RankGiftsAsync_PropertyTests_TiedScoresGetSameRank()
    {
        Prop.ForAll(
            GiftScoreGenerators.ArbitraryScores(),
            validScores =>
            {
                if (validScores == null || validScores.Count == 0)
                    return true;
                
                var result = new SurveyResult { Scores = validScores };
                result.RankGiftsAsync().Wait();
                
                // Invariant: All gifts with the same score must have the same rank
                var scoreGroups = result.Scores.GroupBy(s => s.Score);
                foreach (var group in scoreGroups)
                {
                    var ranks = group.Select(g => g.GiftRank).Distinct().ToList();
                    if (ranks.Count != 1)
                        return false; // All tied scores should have identical rank
                }
                
                return true;
            }
        ).QuickCheckThrowOnFailure();
    }

    [Fact]
    public void RankGiftsAsync_PropertyTests_AllGiftsGetAssignedRank()
    {
        Prop.ForAll(
            GiftScoreGenerators.ArbitraryScores(),
            validScores =>
            {
                if (validScores == null || validScores.Count == 0)
                    return true;
                
                var result = new SurveyResult { Scores = validScores };
                result.RankGiftsAsync().Wait();
                
                // Invariant: Every gift must have a rank (Primary, Secondary, or None)
                var validRanks = new[] { GiftRank.Primary, GiftRank.Secondary, GiftRank.None };
                return result.Scores.All(s => validRanks.Contains(s.GiftRank));
            }
        ).QuickCheckThrowOnFailure();
    }

    [Fact]
    public void RankGiftsAsync_PropertyTests_HigherScoresGetBetterOrEqualRank()
    {
        Prop.ForAll(
            GiftScoreGenerators.ArbitraryScores(),
            validScores =>
            {
                if (validScores == null || validScores.Count < 2)
                    return true;
                
                var result = new SurveyResult { Scores = validScores };
                result.RankGiftsAsync().Wait();
                
                // Invariant: Gift with higher score should NEVER have worse rank than gift with lower score
                // (Equal scores can have same rank, higher scores can have same or better rank)
                var ordered = result.Scores.OrderByDescending(s => s.Score).ToList();
                for (int i = 0; i < ordered.Count - 1; i++)
                {
                    var current = ordered[i];
                    var next = ordered[i + 1];
                    
                    // If current score > next score, current rank must be better or equal
                    if (current.Score > next.Score)
                    {
                        var currentRankValue = RankToValue(current.GiftRank);
                        var nextRankValue = RankToValue(next.GiftRank);
                        
                        // Current should have better (lower value) or equal rank
                        if (currentRankValue > nextRankValue)
                            return false; // Higher score has worse rank = BUG
                    }
                    // If scores are tied, ranks must be identical
                    else if (current.Score == next.Score)
                    {
                        if (current.GiftRank != next.GiftRank)
                            return false; // Tied scores with different ranks = BUG
                    }
                }
                
                return true;
            }
        ).QuickCheckThrowOnFailure();
    }

    [Fact]
    public void RankGiftsAsync_PropertyTests_IdempotentProducesConsistentRankings()
    {
        Prop.ForAll(
            GiftScoreGenerators.ArbitraryScores(),
            validScores =>
            {
                if (validScores == null || validScores.Count == 0)
                    return true;
                
                var result = new SurveyResult { Scores = validScores };
                
                // Call RankGiftsAsync multiple times
                result.RankGiftsAsync().Wait();
                var firstRanking = result.Scores.Select(s => new { s.Gift, s.GiftRank }).ToList();
                
                result.RankGiftsAsync().Wait();
                var secondRanking = result.Scores.Select(s => new { s.Gift, s.GiftRank }).ToList();
                
                // Invariant: Multiple calls should produce identical results
                if (firstRanking.Count != secondRanking.Count)
                    return false;
                
                for (int i = 0; i < firstRanking.Count; i++)
                {
                    if (firstRanking[i].Gift != secondRanking[i].Gift ||
                        firstRanking[i].GiftRank != secondRanking[i].GiftRank)
                        return false;
                }
                
                return true;
            }
        ).QuickCheckThrowOnFailure();
    }

    // Helper method for rank comparison
    private static int RankToValue(GiftRank rank) => rank switch
    {
        GiftRank.Primary => 1,
        GiftRank.Secondary => 2,
        GiftRank.None => 3,
        _ => 4
    };
}

// ============================================================================
// FSCHECK GENERATORS - Custom generators for property-based tests
// ============================================================================
public static class GiftScoreGenerators
{
    /// <summary>
    /// Creates an Arbitrary for generating valid UserGiftScore lists.
    /// Ensures: unique gifts, valid score range (0-90), reasonable list sizes.
    /// </summary>
    public static Arbitrary<List<UserGiftScore>> ArbitraryScores()
    {
        return Arb.From(GenerateValidScores());
    }

    /// <summary>
    /// Generates valid UserGiftScore lists for property-based testing.
    /// Ensures: unique gifts, valid score range (0-90), reasonable list sizes.
    /// </summary>
    public static Gen<List<UserGiftScore>> GenerateValidScores()
    {
        return from count in Gen.Choose(1, 28) // 1-28 gifts (realistic range)
               from scores in Gen.ListOf(count, GenerateSingleScore())
               select MakeUnique(scores.ToList());
    }

    private static Gen<UserGiftScore> GenerateSingleScore()
    {
        return from gift in Gen.Elements(Enum.GetValues<Gifts>())
               from score in Gen.Choose(0, 90) // Realistic score range (30 questions × 3 max)
               select new UserGiftScore { Gift = gift, Score = score };
    }

    // Ensure unique gifts (no duplicates)
    private static List<UserGiftScore> MakeUnique(List<UserGiftScore> scores)
    {
        return scores
            .GroupBy(s => s.Gift)
            .Select(g => g.First())
            .ToList();
    }
}
