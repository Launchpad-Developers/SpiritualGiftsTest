using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using SpiritualGiftsTest.Resources;
using SpiritualGiftsTest.Services;
using SpiritualGiftsTest.ViewModels;
using SpiritualGiftsTest.Views.AppInfo;
using SpiritualGiftsTest.Views.Reporting;
using SpiritualGiftsTest.Views.Results;
using SpiritualGiftsTest.Views.Send;
using SpiritualGiftsTest.Views.Settings;
using SpiritualGiftsTest.Views.Survey;
using SpiritualGiftsTest.Views.Welcome;
using System.Globalization;

namespace SpiritualGiftsTest;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("FA6Pro-Thin.ttf", "FA6ProThin");
                fonts.AddFont("Kanit-SemiBold.ttf", "Kanit");
                fonts.AddFont("MavenPro-Regular.ttf", "MavenPro");
            })
            .RegisterAppServices()
            .RegisterViewModels();

        // Localization
        var deviceCulture = CultureInfo.CurrentCulture;
        AppResources.Culture = deviceCulture;
        CultureInfo.DefaultThreadCurrentCulture = deviceCulture;
        CultureInfo.DefaultThreadCurrentUICulture = deviceCulture;

        // SQLite Init
        SQLitePCL.Batteries.Init();

        return builder.Build();
    }

    public static MauiAppBuilder RegisterAppServices(this MauiAppBuilder mauiAppBuilder)
    {

#if DEBUG
        var baseUrl = "https://sgt-dev-b29c8-default-rtdb.firebaseio.com/";
#else
        var baseUrl = "https://sgt-prod-691ce-default-rtdb.firebaseio.com/";
#endif

        // Register typed HttpClient for URLService
        mauiAppBuilder.Services.AddHttpClient<IURLService, URLService>(client =>
        {
            client.BaseAddress = new Uri(baseUrl ?? throw new InvalidOperationException("BaseUrl missing."));
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // Other app services
        mauiAppBuilder.Services.AddSingleton<IPreferences>(_ => Preferences.Default);
        mauiAppBuilder.Services.AddSingleton<ITranslationService, TranslationService>();
        mauiAppBuilder.Services.AddTransient<IDeviceStorageService, DeviceStorageService>();
        mauiAppBuilder.Services.AddTransient<IDatabaseService, DatabaseService>();
        mauiAppBuilder.Services.AddSingleton<INavigationService, NavigationService>();
        mauiAppBuilder.Services.AddSingleton<IAnalyticsService, AppInsightsService>();
        mauiAppBuilder.Services.AddSingleton<IAggregatedServices, AggregatedServices>();

        return mauiAppBuilder;
    }

    public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddSingleton<NavigationPage>();

        mauiAppBuilder.Services.AddSingleton<SplashScreenViewModel>();

        mauiAppBuilder.Services.AddSingleton<AppInfoViewModel>();
        mauiAppBuilder.Services.AddSingleton<AppInfoPage>();

        mauiAppBuilder.Services.AddSingleton<ReportingViewModel>();
        mauiAppBuilder.Services.AddSingleton<ReportingPage>();

        mauiAppBuilder.Services.AddSingleton<ResultsViewModel>();
        mauiAppBuilder.Services.AddSingleton<ResultsPage>();

        mauiAppBuilder.Services.AddSingleton<SendViewModel>();
        mauiAppBuilder.Services.AddSingleton<SendPage>();

        mauiAppBuilder.Services.AddSingleton<SettingsViewModel>();
        mauiAppBuilder.Services.AddSingleton<SettingsPage>();

        mauiAppBuilder.Services.AddSingleton<SurveyViewModel>();
        mauiAppBuilder.Services.AddSingleton<SurveyPage>();

        mauiAppBuilder.Services.AddSingleton<WelcomeViewModel>();
        mauiAppBuilder.Services.AddSingleton<WelcomePage>();

        return mauiAppBuilder;
    }
}
