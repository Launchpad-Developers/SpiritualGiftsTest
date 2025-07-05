using SpiritualGiftsTest.Services;
using SpiritualGiftsTest.Views.Shared;

namespace SpiritualGiftsTest.Views.Send;

public partial class SendViewModel : BaseViewModel
{
    public SendViewModel(
        IAggregatedServices aggregatedServices,
        IPreferences preferences) : base(aggregatedServices, preferences)
    {

    }
}
