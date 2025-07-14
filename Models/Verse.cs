using SQLite;

namespace SpiritualGiftsSurvey.Models;

[Table("Verse")]
public class Verse
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public Guid VerseGuid { get; set; }
    public Guid GiftDescriptionGuid { get; set; }
    public string Reference { get; set; } = string.Empty;
}
