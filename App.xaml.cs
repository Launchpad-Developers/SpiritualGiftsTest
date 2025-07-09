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

        // Make AppShell the MainPage
        MainPage = Services.GetRequiredService<AppShell>();
    }

    public IServiceProvider Services { get; }


    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = Services.GetRequiredService<AppShell>();
        return new Window(shell);
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