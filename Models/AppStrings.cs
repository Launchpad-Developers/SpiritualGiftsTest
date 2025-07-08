using SQLite;

namespace SpiritualGiftsSurvey.Models;

[Table("AppString")]
public class AppString
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public Guid TranslationGuid { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}
