using SQLite;

namespace SpiritualGiftsTest.Models;

[Table("Question")]
public class Question
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public Guid QuestionGuid { get; set; }
    public string Gift { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
}
