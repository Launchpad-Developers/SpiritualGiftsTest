using SQLite;

namespace SpiritualGiftsSurvey.Models;

[Table("Verse")]
public class Verse
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public Guid VerseGuid { get; set; }
    public string VerseText { get; set; } = string.Empty;

    public Guid GiftDescriptionGuid { get; set; }
}
