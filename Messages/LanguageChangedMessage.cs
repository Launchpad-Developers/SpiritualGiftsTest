using CommunityToolkit.Mvvm.Messaging.Messages;
using SpiritualGiftsTest.Models;

namespace SpiritualGiftsTest.Messages;

public class LanguageChangedMessage : ValueChangedMessage<LanguageOption>
{
    public LanguageChangedMessage(LanguageOption language)
        : base(language)
    {
    }
}

