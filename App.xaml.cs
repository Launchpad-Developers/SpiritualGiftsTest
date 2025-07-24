using System.Diagnostics;

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
        try
        {
            var shell = Services.GetRequiredService<AppShell>();
            return new Window(shell);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Startup crash: {ex}");
            throw;
        }
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