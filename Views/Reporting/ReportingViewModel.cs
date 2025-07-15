using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Views.Shared;

namespace SpiritualGiftsSurvey.Views.Reporting;

public partial class ReportingViewModel : BaseViewModel
{
    public ReportingViewModel(
        IAggregatedServices aggregatedServices,
        IPreferences preferences) : base(aggregatedServices, preferences)
    {

    }

    public override void RefreshViewModel()
    {
        return;
    }

    public override void InitAsync()
    {
        return;
    }
}
