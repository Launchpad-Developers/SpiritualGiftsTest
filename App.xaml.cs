namespace SpiritualGiftsTest;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        AppResources.Culture = CrossMultilingual.Current.DeviceCultureInfo;

        var navPage = new NavigationPage(new Views.Welcome.WelcomePage());
        MainPage = navPage;
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