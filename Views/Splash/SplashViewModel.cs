using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Views.Shared;

namespace SpiritualGiftsSurvey.Views.Splash;

public class SplashViewModel : BaseViewModel
{
    public SplashViewModel(
        IAggregatedServices aggregatedServices,
        IPreferences preferences)
        : base(aggregatedServices, preferences)
    {
    }

    public async override Task InitAsync()
    {
        await Task.Yield();
        
        return;
    }

    public override void RefreshViewModel()
    {
        return;
    }
}