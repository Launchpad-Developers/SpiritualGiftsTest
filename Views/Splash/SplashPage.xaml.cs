using SpiritualGiftsSurvey.Views.Shared;

namespace SpiritualGiftsSurvey.Views.Splash;

public partial class SplashPage : BasePage
{
	public SplashPage(SplashViewModel splashViewModel)
        : base(splashViewModel)
    {
		InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.InitAsync(Navigation);
    }
}