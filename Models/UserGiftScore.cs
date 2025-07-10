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
}
