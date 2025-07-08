using SQLite;

namespace SpiritualGiftsSurvey.Models;

[Table("Reflection")]
public class Reflection
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public Guid ReflectionGuid { get; set; }
    public int Number { get; set; }
    public string Question { get; set; } = string.Empty;
}
