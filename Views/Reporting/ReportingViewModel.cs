using SpiritualGiftsTest.Services;
using SpiritualGiftsTest.Views.Shared;

namespace SpiritualGiftsTest.Views.Reporting;

public class ReportingViewModel : BaseViewModel
{
    public ReportingViewModel(
        IAggregatedServices aggregatedServices,
        IPreferences preferences) : base(aggregatedServices, preferences)
    {

    }
}
