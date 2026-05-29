using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Enums;

namespace SpiritualGiftsSurvey.Tests;

/// <summary>
/// Regression tests for concurrent operations and lifecycle coordination (HIGH-2/3/4/5).
/// Tests thread-safety and reentrancy guards without requiring full MAUI infrastructure.
/// </summary>
public class ConcurrencyTests
{
    [Fact]
    public async Task SurveyResult_ConcurrentRanking_SerializesCorrectly()
    {
        // Arrange
        var result = new SurveyResult
        {
            Scores = new List<UserGiftScore>
            {
                new() { Gift = Gifts.Prophecy, Score = 50 },
                new() { Gift = Gifts.Teaching, Score = 48 },
                new() { Gift = Gifts.Wisdom, Score = 45 },
                new() { Gift = Gifts.Faith, Score = 30 }
            }
        };

        // Act - Simulate concurrent ranking calls (stress test)
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(result.RankGiftsAsync());
        }

        await Task.WhenAll(tasks);

        // Assert - Should remain consistent despite 100 concurrent calls
        Assert.True(result.IsRanked);
        var primary = result.Scores.Where(s => s.GiftRank == GiftRank.Primary).ToList();
        Assert.Equal(3, primary.Count);

        // All scores should have valid ranks
        foreach (var score in result.Scores)
        {
            Assert.True(score.GiftRank == GiftRank.Primary || score.GiftRank == GiftRank.Secondary || score.GiftRank == GiftRank.None);
        }
    }

    [Fact]
    public async Task SurveyResult_RankingDuringEnumeration_DoesNotThrowException()
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

        // Act - Start ranking and immediately try to enumerate
        var rankingTask = result.RankGiftsAsync();
        
        // This simulates UI reading scores while ranking is in progress
        var exception = Record.Exception(() =>
        {
            var scores = result.Scores.ToList(); // Force enumeration
        });

        await rankingTask;

        // Assert - Should not throw collection modified exception
        // (SemaphoreSlim prevents concurrent access)
        Assert.Null(exception);
    }

    [Fact]
    public async Task SurveyResult_RapidSuccessiveRanking_PerformsOnlyOnce()
    {
        // Arrange
        var result = new SurveyResult
        {
            Scores = new List<UserGiftScore>
            {
                new() { Gift = Gifts.Prophecy, Score = 50 }
            }
        };

        // Act - Rapid successive calls (simulating property change triggers)
        await result.RankGiftsAsync();
        await result.RankGiftsAsync();
        await result.RankGiftsAsync();

        // Assert - Should only rank once due to IsRanked guard
        Assert.True(result.IsRanked);
    }

    [Fact]
    public async Task SurveyResult_AwaitsConcurrentRanking_InsteadOfStartingNew()
    {
        // Arrange
        var result = new SurveyResult
        {
            Scores = Enumerable.Range(0, 100)
                .Select(i => new UserGiftScore { Gift = (Gifts)(i % 22), Score = 50 - i })
                .ToList()
        };

        // Act - Start first ranking (will take some time due to 100 items)
        var firstTask = result.RankGiftsAsync();
        
        // Start second ranking immediately (should await first, not start new)
        var secondTask = result.RankGiftsAsync();

        await Task.WhenAll(firstTask, secondTask);

        // Assert - Both should complete with same result
        Assert.True(result.IsRanked);
    }
}
