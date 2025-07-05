using SpiritualGiftsTest.Services;
using SpiritualGiftsTest.Views.Shared;

namespace SpiritualGiftsTest.ViewModels;


public class SplashScreenViewModel : BaseViewModel
{
    public SplashScreenViewModel(
        IAggregatedServices aggregatedServices, 
        IPreferences preferences) 
            : base(aggregatedServices, preferences)
    {
    }

    public async Task InitializeAsync()
    {
        await Task.Delay(4000);
        await NavigationService.NavigateAsync("/NavigationPage/WelcomePage");
    }
}