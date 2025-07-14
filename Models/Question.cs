using SpiritualGiftsSurvey.Enums;
using SQLite;

namespace SpiritualGiftsSurvey.Models;

[Table("Question")]
public class Question
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public Guid QuestionGuid { get; set; }
    public Guid TranslationGuid { get; set; }
    public Gifts Gift { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public Guid GiftDescriptionGuid { get; set; }
}
