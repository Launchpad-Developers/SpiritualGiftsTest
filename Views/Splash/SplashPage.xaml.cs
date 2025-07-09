using SpiritualGiftsSurvey.Routing;
using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Views.Shared;

namespace SpiritualGiftsSurvey.Views.Splash;

public partial class SplashPage : BasePage
{
    private readonly INavigationService _navigationService;

    public SplashPage(
        SplashViewModel splashViewModel, 
        INavigationService navigationService)
        : base(splashViewModel)
    {
        _navigationService = navigationService;
        BindingContext = splashViewModel;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(4000);
        await _navigationService.NavigateAsync(Routes.WelcomePage);
    }
}