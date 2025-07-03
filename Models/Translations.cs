using System.Collections.Generic;

namespace SpiritualGiftsTest.Models;

public class Translations
{
    public Translations()
    {
        TranslationCollection = new List<TranslationModel>();
    }

	public Translations(List<TranslationModel> translations)
    {
	    TranslationCollection = translations;
    }

    public List<TranslationModel> TranslationCollection { get; set; }
}
