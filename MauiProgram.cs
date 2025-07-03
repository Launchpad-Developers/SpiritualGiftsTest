using Microsoft.Extensions.Logging;
using SpiritualGiftsTest.Interfaces;
using SpiritualGiftsTest.Services;
using SpiritualGiftsTest.Views.AppInfo;
using SpiritualGiftsTest.Views.Shared;
using SpiritualGiftsTest.Views.Welcome;

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

        return builder.Build();
    }

    public static MauiAppBuilder RegisterAppServices(this MauiAppBuilder mauiAppBuilder)
    {
        //Do not remove qualifying domain from Network or this will not build
        mauiAppBuilder.Services.AddSingleton<ITranslationService, TranslationService>();
        mauiAppBuilder.Services.AddSingleton<IURLService, URLService>();
        mauiAppBuilder.Services.AddTransient<IDatabaseService, DatabaseService>();
        mauiAppBuilder.Services.AddSingleton<IAggregatedServices, AggregatedServices>();
        mauiAppBuilder.Services.AddSingleton<INavigationService, NavigationService>();

        return mauiAppBuilder;
    }

    public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddSingleton<NavigationPage>();
        //mauiAppBuilder.Services.AddSingleton<BasePage>(); Cannot instantiate abstract
        mauiAppBuilder.Services.AddSingleton<BaseViewModel>();

        mauiAppBuilder.Services.AddSingleton<AppInfoPage>();
        mauiAppBuilder.Services.AddSingleton<AppInfoViewModel>();

        mauiAppBuilder.Services.AddSingleton<ReportingPage>();
        mauiAppBuilder.Services.AddSingleton<ReportingViewModel>();

        mauiAppBuilder.Services.AddSingleton<ResultsPage>();
        mauiAppBuilder.Services.AddSingleton<ResultsViewModel>();

        mauiAppBuilder.Services.AddSingleton<SendPage>();
        mauiAppBuilder.Services.AddSingleton<SendViewModel>();

        mauiAppBuilder.Services.AddSingleton<SettingsPage>();
        mauiAppBuilder.Services.AddSingleton<SettingsViewModel>();

        mauiAppBuilder.Services.AddSingleton<WelcomePage>();
        mauiAppBuilder.Services.AddSingleton<WelcomeViewModel>();

        mauiAppBuilder.Services.AddSingleton<SplashScreenViewModel>();

        return mauiAppBuilder;
    }
}
