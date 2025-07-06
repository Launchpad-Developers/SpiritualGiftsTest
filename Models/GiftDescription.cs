using SQLite;

namespace SpiritualGiftsTest.Models;

[Table("GiftDescription")]
public class GiftDescription
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public Guid GiftDescriptionGuid { get; set; }
    public string Gift { get; set; } = string.Empty;
    public string Translation { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Verse> Verses { get; set; } = new();
}
