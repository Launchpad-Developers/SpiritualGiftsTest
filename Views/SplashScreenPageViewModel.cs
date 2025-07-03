using System.Threading.Tasks;
using SpiritualGiftsTest.Interfaces;
using SpiritualGiftsTest.Views.Shared;

namespace SpiritualGiftsTest.ViewModels;


public class SplashScreenViewModel : BaseViewModel
{
    public SplashScreenViewModel(IAggregatedServices aggregatedServices) : base(aggregatedServices)
    {
    }

    public async Task InitializeAsync()
    {
        await Task.Delay(4000);
        await NavigationService.NavigateAsync("/NavigationPage/WelcomePage");
    }
}