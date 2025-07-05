using SpiritualGiftsTest.Services;
using SpiritualGiftsTest.Views.Shared;

namespace SpiritualGiftsTest.Views.Reporting;

public partial class ReportingViewModel : BaseViewModel
{
    public ReportingViewModel(
        IAggregatedServices aggregatedServices,
        IPreferences preferences) : base(aggregatedServices, preferences)
    {

    }
}
