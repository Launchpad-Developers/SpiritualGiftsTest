using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SpiritualGiftsTest.Models;

namespace SpiritualGiftsTest.Interfaces;

public interface IDatabaseService
{
	Task<TranslationModel> GetTranslationForCode(string code);
	IEnumerable<TranslationOptionModel> GetCurrentTranslationOptions(string languageCode);
}
