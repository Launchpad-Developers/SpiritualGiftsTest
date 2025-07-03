
namespace SpiritualGiftsTest.Interfaces;

public interface IAggregatedServices
{
    IDatabaseService DatabaseService { get; }
    ITranslationService TranslationService { get; }
    IURLService URLService { get; }
    IDeviceStorageService DeviceStorageService { get; }
    INavigationService NavigationService { get; }
}
