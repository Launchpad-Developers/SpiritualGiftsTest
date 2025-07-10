namespace SpiritualGiftsSurvey.Services;

public interface IAggregatedServices
{
    IDatabaseService DatabaseService { get; }
    ITranslationService TranslationService { get; }
    IURLService URLService { get; }
    IDeviceStorageService DeviceStorageService { get; }
    INavigationService NavigationService { get; }
    IAnalyticsService AnalyticsService { get; }
    IAppInfoService AppInfoService { get; }
}

public class AggregatedServices : IAggregatedServices
{
    public IDatabaseService DatabaseService { get; }
    public ITranslationService TranslationService { get; }
    public IURLService URLService { get; }
    public IDeviceStorageService DeviceStorageService { get; }
    public INavigationService NavigationService { get; }
    public IAnalyticsService AnalyticsService { get; }
    public IAppInfoService AppInfoService { get; }

    public AggregatedServices(IDatabaseService databaseService,
                              ITranslationService translationService,
                              IURLService urlService,
                              IDeviceStorageService deviceStorageService,
                              INavigationService navigationService,
                              IAnalyticsService analyticsService,
                              IAppInfoService appInfoService)
    {
        DatabaseService = databaseService;
        TranslationService = translationService;
        URLService = urlService;
        DeviceStorageService = deviceStorageService;
        NavigationService = navigationService;
        AnalyticsService = analyticsService;
        AppInfoService = appInfoService;
    }
}
