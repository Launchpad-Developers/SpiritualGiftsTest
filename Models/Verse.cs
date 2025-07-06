using SQLite;

namespace SpiritualGiftsTest.Models;

[Table("Verse")]
public class Verse
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public Guid VerseGuid { get; set; }
    public string VerseText { get; set; } = string.Empty;
}
