using SpiritualGiftsSurvey.Enums;
using SQLite;

namespace SpiritualGiftsSurvey.Models;

[Table("UserGiftScores")]
public class UserGiftScore
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public Guid UserGiftResultGuid { get; set; }
    public string GiftName { get; set; } = string.Empty;
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public GiftRank GiftRank { get; set; }
    public Guid GiftDescriptionGuid { get; set; }

    [Ignore]
    public double Progress => MaxScore > 0 ? (double)Score / MaxScore : 0;
    [Ignore]
    public Gifts Gift { get; set; }

}
