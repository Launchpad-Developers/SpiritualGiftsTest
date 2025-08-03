using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Views.Shared;

namespace SpiritualGiftsSurvey.Views.Splash;

public class SplashViewModel(
    IAggregatedServices aggregatedServices,
    IPreferences preferences)
    : BaseViewModel(aggregatedServices, preferences)
{
    public override async Task InitAsync()
    {
        await Task.Yield();
        
        return;
    }

    public override void RefreshViewModel()
    {
        return;
    }
}