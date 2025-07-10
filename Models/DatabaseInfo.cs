using SQLite;

namespace SpiritualGiftsSurvey.Models;

	
[Table("DatabaseInfo")]
public class DatabaseInfo
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Date { get; set; } = string.Empty;
    public int Version { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
