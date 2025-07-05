using CommunityToolkit.Maui;
using SpiritualGiftsTest.Resources;
using SpiritualGiftsTest.Services;
using SpiritualGiftsTest.ViewModels;
using SpiritualGiftsTest.Views.AppInfo;
using SpiritualGiftsTest.Views.Reporting;
using SpiritualGiftsTest.Views.Results;
using SpiritualGiftsTest.Views.Send;
using SpiritualGiftsTest.Views.Settings;
using SpiritualGiftsTest.Views.Test;
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

        var deviceCulture = CultureInfo.CurrentCulture;

        AppResources.Culture = deviceCulture;
        CultureInfo.DefaultThreadCurrentCulture = deviceCulture;
        CultureInfo.DefaultThreadCurrentUICulture = deviceCulture;

        SQLitePCL.Batteries.Init();

        return builder.Build();
    }

    public static MauiAppBuilder RegisterAppServices(this MauiAppBuilder mauiAppBuilder)
    {
        //Do not remove qualifying domain from Network or this will not build
        mauiAppBuilder.Services.AddSingleton<IPreferences>(_ => Preferences.Default);
        mauiAppBuilder.Services.AddSingleton<ITranslationService, TranslationService>();
        mauiAppBuilder.Services.AddSingleton<IURLService, URLService>();
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

        mauiAppBuilder.Services.AddSingleton<TestViewModel>();
        mauiAppBuilder.Services.AddSingleton<TestPage>();

        mauiAppBuilder.Services.AddSingleton<WelcomeViewModel>();
        mauiAppBuilder.Services.AddSingleton<WelcomePage>();

        return mauiAppBuilder;
    }
}
