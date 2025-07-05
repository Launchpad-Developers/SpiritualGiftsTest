using SQLite;

namespace SpiritualGiftsTest.Models;

	
[Table("TranslationOption")]
public class TranslationOptionModel
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }

	public string Code { get; set; } = string.Empty;

	public string CodeOption { get; set; } = string.Empty;

	public string CodeOptionTranslation { get; set; } = string.Empty;

	public bool Selected { get; set; }
}
