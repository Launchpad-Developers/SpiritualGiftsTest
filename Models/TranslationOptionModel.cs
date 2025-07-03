using SQLite;

namespace SpiritualGiftsTest.Models;

	
[Table("TranslationOption")]
public class TranslationOptionModel
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }

		public string Code { get; set; }

		public string CodeOption { get; set; }

		public string CodeOptionTranslation { get; set; }

		public bool Selected { get; set; }
}
