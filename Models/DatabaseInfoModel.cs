using SQLite;

namespace SpiritualGiftsTest.Models;

	
[Table("DatabaseInfo")]
public class DatabaseInfoModel
{
    public int Version { get; set; }
}
