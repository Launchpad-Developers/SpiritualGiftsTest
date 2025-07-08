using SpiritualGiftsSurvey.Resources;
using SpiritualGiftsSurvey.Views.Splash;
using SpiritualGiftsSurvey.Views.Welcome;

namespace SpiritualGiftsSurvey;

public partial class App : Application
{
    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        Services = serviceProvider;
    }

    public IServiceProvider Services { get; }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var splashPage = Services.GetRequiredService<SplashPage>();
        var navPage = new NavigationPage(splashPage);
        return new Window(navPage);
    }

    protected override void OnStart()
    {

    }

    protected override void OnSleep()
    {
        // Handle IApplicationLifecycle
        base.OnSleep();

        // Handle when your app sleeps
    }

    protected override void OnResume()
    {
        // Handle IApplicationLifecycle
        base.OnResume();

        // Handle when your app resumes
    }
}