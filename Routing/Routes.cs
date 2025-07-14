namespace SpiritualGiftsSurvey.Routing;

public static class Routes
{
    // Use nameof so refactoring never breaks routes

    public static string SplashPage => nameof(SplashPage);
    public static string WelcomePage => nameof(WelcomePage);
    public static string SurveyPage => nameof(SurveyPage);
    public static string ResultsPage => nameof(ResultsPage);
    public static string GiftDescriptionPage => nameof(GiftDescriptionPage);
    public static string ReportingPage => nameof(ReportingPage);
    public static string SendPage => nameof(SendPage);
    public static string SettingsPage => nameof(SettingsPage);
    public static string AppInfoPage => nameof(AppInfoPage);
}
