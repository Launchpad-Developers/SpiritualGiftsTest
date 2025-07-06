using SQLite;

namespace SpiritualGiftsTest.Models;

	
[Table("DatabaseInfo")]
public class DatabaseInfo
{
    public string Date { get; set; } = string.Empty;
    public int Version { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
