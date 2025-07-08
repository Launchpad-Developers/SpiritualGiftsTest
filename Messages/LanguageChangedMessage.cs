using CommunityToolkit.Mvvm.Messaging.Messages;
using SpiritualGiftsSurvey.Models;

namespace SpiritualGiftsSurvey.Messages;

public class LanguageChangedMessage : ValueChangedMessage<bool>
{
    public LanguageChangedMessage(bool value) : base(value)
    {
    }
}

