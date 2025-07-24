using SpiritualGiftsSurvey.Enums;
using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Utilities;

namespace SpiritualGiftsSurvey.Helpers;

public static class DebugHelper
{
    public static Dictionary<Guid, UserValue> ApplyDebugQuestionFilters(ref List<Question> questions, Random random)
    {
        var topicLimit = Preferences.Get(AppConstants.DebugTotalTopicsKey, int.MaxValue);
        var questionsPerTopic = Preferences.Get(AppConstants.DebugQuestionsPerTopicKey, int.MaxValue);
        var unansweredLimit = Preferences.Get(AppConstants.DebugAllowUnansweredQuestionsKey, false) ?
                              Preferences.Get(AppConstants.DebugTotalUnansweredQuestionsKey, 0) : 0;

        var giftCounter = new Dictionary<Gifts, int>();
        var debugUserValues = new Dictionary<Guid, UserValue>();

        // 1. Select only the first N gifts
        var selectedGifts = questions
            .Select(q => q.Gift)
            .Distinct()
            .Take(topicLimit)
            .ToHashSet();

        // 2. Filter questions by gift/topic limit
        questions = questions
            .Where(q =>
            {
                if (!selectedGifts.Contains(q.Gift))
                    return false;

                if (!giftCounter.ContainsKey(q.Gift))
                    giftCounter[q.Gift] = 0;

                if (giftCounter[q.Gift] < questionsPerTopic)
                {
                    giftCounter[q.Gift]++;
                    return true;
                }

                return false;
            })
            .ToList();

        // 3. Randomly assign DidNotAnswer to some
        if (unansweredLimit > 0)
        {
            var availableIndexes = Enumerable.Range(0, questions.Count).ToList();
            for (int i = 0; i < unansweredLimit && availableIndexes.Count > 0; i++)
            {
                int index = random.Next(availableIndexes.Count);
                int questionIndex = availableIndexes[index];
                var question = questions[questionIndex];

                debugUserValues[question.QuestionGuid] = UserValue.DidNotAnswer;

                availableIndexes.RemoveAt(index);
            }
        }

        // 4. Assign random answers to remaining unanswered
        foreach (var question in questions)
        {
            if (!debugUserValues.ContainsKey(question.QuestionGuid))
            {
                var possibleValues = Enum.GetValues<UserValue>()
                    .Where(v => v != UserValue.DidNotAnswer)
                    .ToList();

                debugUserValues[question.QuestionGuid] = possibleValues[random.Next(possibleValues.Count)];
            }
        }

        return debugUserValues;
    }
}
