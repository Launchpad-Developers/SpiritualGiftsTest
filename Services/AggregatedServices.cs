using SpiritualGiftsTest.Interfaces;

namespace SpiritualGiftsTest.Services;

public class AggregatedServices : IAggregatedServices
{
    public IDatabaseService DatabaseService { get; }
    public ITranslationService TranslationService { get; }
    public IURLService URLService { get; }
    public IDeviceStorageService DeviceStorageService { get; }

    public INavigationService NavigationService { get; }

    public AggregatedServices(IDatabaseService databaseService,
                              ITranslationService translationService,
                              IURLService urlService,
                              IDeviceStorageService deviceStorageService,
                              INavigationService navigationService)
    {
        DatabaseService = databaseService;
        TranslationService = translationService;
        URLService = urlService;
        DeviceStorageService = deviceStorageService;
        NavigationService = navigationService;
    }
}
