using CommunityToolkit.Mvvm.Messaging.Messages;
using SpiritualGiftsTest.Models;

namespace SpiritualGiftsTest.Messages;

public class LanguageChangedMessage : ValueChangedMessage<TranslationOptionModel>
{
    public LanguageChangedMessage(TranslationOptionModel language)
        : base(language)
    {
    }
}

