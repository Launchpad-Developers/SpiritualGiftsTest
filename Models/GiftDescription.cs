using SpiritualGiftsSurvey.Enums;
using SQLite;

namespace SpiritualGiftsSurvey.Models;

[Table("GiftDescription")]
public class GiftDescription
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public Guid GiftDescriptionGuid { get; set; }
    public Guid TranslationGuid { get; set; }
    public Gifts Gift { get; set; }
    public string Translation { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [Ignore] public List<Verse> Verses { get; set; } = new();
}
