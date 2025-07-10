using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SpiritualGiftsSurvey.Messages;

public class ScrollToQuestionMessage : ValueChangedMessage<int>
{
    public ScrollToQuestionMessage(int index) : base(index) { }
}
