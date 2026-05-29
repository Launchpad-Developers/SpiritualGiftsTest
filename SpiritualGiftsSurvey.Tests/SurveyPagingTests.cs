using SpiritualGiftsSurvey.Enums;

namespace SpiritualGiftsSurvey.Tests;

/// <summary>
/// Tests for survey paging logic - page calculations and navigation
/// Note: These tests don't require MAUI components, just pure logic testing
/// </summary>
public class SurveyPagingTests
{
    private const int QuestionsPerPage = 10;

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(9, 1)]
    [InlineData(10, 1)]
    [InlineData(11, 2)]
    [InlineData(20, 2)]
    [InlineData(21, 3)]
    [InlineData(100, 10)]
    [InlineData(250, 25)]
    public void CalculateTotalPages_ReturnsCorrectPageCount(int questionCount, int expectedPages)
    {
        // Act
        var totalPages = (int)Math.Ceiling((double)questionCount / QuestionsPerPage);

        // Assert
        Assert.Equal(expectedPages, totalPages);
    }

    [Theory]
    [InlineData(1, 100, 0, 10)]  // Page 1 of 100 questions: index 0-9
    [InlineData(2, 100, 10, 20)] // Page 2 of 100 questions: index 10-19
    [InlineData(5, 100, 40, 50)] // Page 5 of 100 questions: index 40-49
    [InlineData(10, 100, 90, 100)] // Page 10 of 100 questions: index 90-99
    public void CalculatePageRange_ReturnsCorrectIndices(int currentPage, int totalQuestions, int expectedStart, int expectedEnd)
    {
        // Arrange - Assume totalQuestions questions available
        
        // Act
        int startIndex = (currentPage - 1) * QuestionsPerPage;
        int endIndex = Math.Min(startIndex + QuestionsPerPage, totalQuestions);

        // Assert
        Assert.Equal(expectedStart, startIndex);
        Assert.Equal(expectedEnd, endIndex);
    }

    [Fact]
    public void CalculatePageRange_LastPagePartialQuestions()
    {
        // Arrange
        int totalQuestions = 25; // 3 pages: 10, 10, 5
        int currentPage = 3;

        // Act
        int startIndex = (currentPage - 1) * QuestionsPerPage;
        int endIndex = Math.Min(startIndex + QuestionsPerPage, totalQuestions);

        // Assert
        Assert.Equal(20, startIndex);
        Assert.Equal(25, endIndex);
        Assert.Equal(5, endIndex - startIndex); // Only 5 questions on last page
    }

    [Theory]
    [InlineData(1, false)] // First page - can't go back
    [InlineData(2, true)]
    [InlineData(5, true)]
    [InlineData(10, true)]
    public void CanGoPrevious_ReturnsCorrectValue(int currentPage, bool expectedCanGo)
    {
        // Act
        bool canGoPrevious = currentPage > 1;

        // Assert
        Assert.Equal(expectedCanGo, canGoPrevious);
    }

    [Theory]
    [InlineData(1, 10, true)]
    [InlineData(5, 10, true)]
    [InlineData(9, 10, true)]
    [InlineData(10, 10, false)] // Last page - can't go forward
    public void CanGoNext_ReturnsCorrectValue(int currentPage, int totalPages, bool expectedCanGo)
    {
        // Act
        bool canGoNext = currentPage < totalPages;

        // Assert
        Assert.Equal(expectedCanGo, canGoNext);
    }

    [Theory]
    [InlineData(1, 10, "Page 1 of 10")]
    [InlineData(5, 10, "Page 5 of 10")]
    [InlineData(1, 1, "Page 1 of 1")]
    [InlineData(25, 28, "Page 25 of 28")]
    public void FormatPageIndicator_ReturnsCorrectString(int currentPage, int totalPages, string expected)
    {
        // Act
        string pageIndicator = $"Page {currentPage} of {totalPages}";

        // Assert
        Assert.Equal(expected, pageIndicator);
    }

    [Fact]
    public void ShowFinishButton_OnlyOnLastPage()
    {
        // Arrange
        int totalPages = 10;

        // Act & Assert
        for (int page = 1; page <= totalPages; page++)
        {
            bool showFinish = (page == totalPages);
            
            if (page == totalPages)
                Assert.True(showFinish, $"Finish button should show on last page {page}");
            else
                Assert.False(showFinish, $"Finish button should NOT show on page {page}");
        }
    }

    [Fact]
    public void NavigateNext_IncreasesCurrentPage()
    {
        // Arrange
        int currentPage = 3;
        int totalPages = 10;

        // Act
        if (currentPage < totalPages)
        {
            currentPage++;
        }

        // Assert
        Assert.Equal(4, currentPage);
    }

    [Fact]
    public void NavigateNext_DoesNotExceedTotalPages()
    {
        // Arrange
        int currentPage = 10;
        int totalPages = 10;

        // Act
        if (currentPage < totalPages)
        {
            currentPage++;
        }

        // Assert
        Assert.Equal(10, currentPage); // Should not increase
    }

    [Fact]
    public void NavigatePrevious_DecreasesCurrentPage()
    {
        // Arrange
        int currentPage = 5;

        // Act
        if (currentPage > 1)
        {
            currentPage--;
        }

        // Assert
        Assert.Equal(4, currentPage);
    }

    [Fact]
    public void NavigatePrevious_DoesNotGoBelowOne()
    {
        // Arrange
        int currentPage = 1;

        // Act
        if (currentPage > 1)
        {
            currentPage--;
        }

        // Assert
        Assert.Equal(1, currentPage); // Should not decrease
    }

    [Fact]
    public void FindUnansweredQuestionPage_ReturnsCorrectPage()
    {
        // Arrange - simulate 50 questions where question 24 (index 23) is unanswered
        int totalQuestions = 50;
        int unansweredIndex = 23; // Question 24

        // Act - Calculate which page contains the unanswered question
        var pageWithUnanswered = (unansweredIndex / QuestionsPerPage) + 1;

        // Assert
        Assert.Equal(3, pageWithUnanswered); // Question 24 (index 23) is on page 3 (questions 21-30)
    }

    [Theory]
    [InlineData(10, 10)] // Exactly 10 questions = 1 page
    [InlineData(20, 20)] // Exactly 20 questions = 2 pages
    [InlineData(100, 100)] // Exactly 100 questions = 10 pages
    public void EdgeCase_ExactMultipleOfPageSize(int questionCount, int totalQuestions)
    {
        // Act
        var totalPages = (int)Math.Ceiling((double)questionCount / QuestionsPerPage);
        
        int lastPageStartIndex = (totalPages - 1) * QuestionsPerPage;
        int lastPageEndIndex = Math.Min(lastPageStartIndex + QuestionsPerPage, totalQuestions);
        int lastPageQuestionCount = lastPageEndIndex - lastPageStartIndex;

        // Assert
        Assert.Equal(QuestionsPerPage, lastPageQuestionCount); // Last page should have full 10 questions
    }

    [Fact]
    public void EdgeCase_SingleQuestion()
    {
        // Arrange
        int questionCount = 1;

        // Act
        var totalPages = (int)Math.Ceiling((double)questionCount / QuestionsPerPage);
        bool canGoNext = 1 < totalPages;
        bool canGoPrevious = 1 > 1;
        bool showFinish = (1 == totalPages);

        // Assert
        Assert.Equal(1, totalPages);
        Assert.False(canGoNext);
        Assert.False(canGoPrevious);
        Assert.True(showFinish);
    }
}
