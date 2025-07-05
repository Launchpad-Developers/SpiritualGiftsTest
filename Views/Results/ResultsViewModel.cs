using SpiritualGiftsTest.Services;
using SpiritualGiftsTest.Views.Shared;

namespace SpiritualGiftsTest.Views.Results;

public partial class ResultsViewModel : BaseViewModel
{
    public ResultsViewModel(
        IAggregatedServices aggregatedServices,
        IPreferences preferences) : base(aggregatedServices, preferences)
    {

    }
}
