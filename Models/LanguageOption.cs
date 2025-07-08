using SQLite;

namespace SpiritualGiftsSurvey.Models;

[Table("LanguageOption")]
public class LanguageOption
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public Guid LanguageOptionGuid { get; set; }
    public string CodeOption { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
