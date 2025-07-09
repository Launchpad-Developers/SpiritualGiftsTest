using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Views.Shared;
using SpiritualGiftsSurvey.Views.Welcome;

namespace SpiritualGiftsSurvey.Views.Splash;


public class SplashViewModel : BaseViewModel
{
    private readonly WelcomePage _welcomePage;

    public SplashViewModel(
        IAggregatedServices aggregatedServices, 
        IPreferences preferences,
        WelcomePage welcomePage)
        : base(aggregatedServices, preferences)
    {
        _welcomePage = welcomePage;
    }
}