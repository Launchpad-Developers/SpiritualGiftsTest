using System.Collections.Generic;

namespace SpiritualGiftsTest.Models;

public class TranslationOptions
{
	public TranslationOptions()
	{
		TranslationOptionCollection = new List<TranslationOptionModel>();
	}

	public TranslationOptions(List<TranslationOptionModel> translations)
	{
		TranslationOptionCollection = translations;
	}

	public List<TranslationOptionModel> TranslationOptionCollection { get; set; }
}
