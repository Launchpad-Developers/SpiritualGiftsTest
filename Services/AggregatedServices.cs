namespace SpiritualGiftsSurvey.Services;

public interface IAggregatedServices
{
    IDatabaseService DatabaseService { get; }
    ITranslationService TranslationService { get; }
    IUrlService URLService { get; }
    IDeviceStorageService DeviceStorageService { get; }
    INavigationService NavigationService { get; }
    IAnalyticsService AnalyticsService { get; }
    IAppInfoService AppInfoService { get; }
    IEmailService EmailService { get; }
    ISurveyProgressService SurveyProgressService { get; }
}

public class AggregatedServices : IAggregatedServices
{
    public IDatabaseService DatabaseService { get; }
    public ITranslationService TranslationService { get; }
    public IUrlService URLService { get; }
    public IDeviceStorageService DeviceStorageService { get; }
    public INavigationService NavigationService { get; }
    public IAnalyticsService AnalyticsService { get; }
    public IAppInfoService AppInfoService { get; }
    public IEmailService EmailService { get; }
    public ISurveyProgressService SurveyProgressService { get; }

    public AggregatedServices(IDatabaseService databaseService,
                              ITranslationService translationService,
                              IUrlService urlService,
                              IDeviceStorageService deviceStorageService,
                              INavigationService navigationService,
                              IAnalyticsService analyticsService,
                              IAppInfoService appInfoService,
                              IEmailService emailService,
                              ISurveyProgressService surveyProgressService)
    {
        DatabaseService = databaseService;
        TranslationService = translationService;
        URLService = urlService;
        DeviceStorageService = deviceStorageService;
        NavigationService = navigationService;
        AnalyticsService = analyticsService;
        AppInfoService = appInfoService;
        EmailService = emailService;
        SurveyProgressService = surveyProgressService;
    }
}
