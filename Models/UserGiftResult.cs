using SQLite;

namespace SpiritualGiftsSurvey.Models;

[Table("UserGiftResults")]
public class UserGiftResult
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public Guid UserGiftResultGuid { get; set; } = Guid.NewGuid();

    public DateTime DateTaken { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    // Optional full raw results snapshot
    public string Results { get; set; } = string.Empty;

    [Ignore]
    public List<UserGiftScore> Scores { get; set; } = new();
}
