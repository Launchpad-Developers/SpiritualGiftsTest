using System.Collections.Generic;
using System.Threading.Tasks;
using SpiritualGiftsTest.Models;

namespace SpiritualGiftsTest.Interfaces;

public interface ITranslationService
{
	TranslationModel PrimaryLanguage { get; }
    TranslationModel ParallelLanguage { get; }

    IEnumerable<TranslationOptionModel> PrimaryTranslationOptions { get; }
    IEnumerable<TranslationOptionModel> ParallelTranslationOptions { get; }

    string PrimaryLanguageCode { get; }
    string ParallelLanguageCode { get; }

    /// <summary>
    /// Initializes the translation.
		/// This should only be called when app first starts or if the Primary language changes.
    /// </summary>
    /// <returns>True if translation could be set, false otherwise.</returns>
	Task<bool> InitializeLanguages();

    /// <summary>
    /// Sets the current translation.
		/// </summary>
    /// <returns>True if translation could be set, false otherwise.</returns>        
	Task<bool> SetPrimaryLanguageForCode(TranslationOptionModel current);

    /// <summary>
    /// Sets the current translation.
    /// </summary>
    /// <returns>True if translation could be set, false otherwise.</returns>        
    Task<bool> SetParallelLanguageForCode(TranslationOptionModel current);
}
