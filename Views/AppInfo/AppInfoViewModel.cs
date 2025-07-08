using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpiritualGiftsSurvey.Helpers;
using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Views.Shared;
using System.Runtime.Versioning;
using System.Windows.Input;

namespace SpiritualGiftsSurvey.Views.AppInfo;

[SupportedOSPlatform("android")]
[SupportedOSPlatform("ios")]
public partial class AppInfoViewModel : BaseViewModel
{
    public AppInfoViewModel(
        IAggregatedServices aggregatedServices,
        IPreferences preferences) : base(aggregatedServices, preferences)
    {
        Initialize();
    }


    [RelayCommand]
    private async Task SendEmailAsync(string emailAddress)
	{
        try
        {
            var message = new EmailMessage
            {
                Subject = "Spiritual Gifts Survey",
                Body = "Information for Spiritual Gifts Survey.",
                To = new List<string>() { emailAddress }
            };
            await Email.ComposeAsync(message);
        }
        catch (FeatureNotSupportedException fnsEx)
        {
            Analytics.TrackEvent("EmailNotSupported", new Dictionary<string, string>() { { "Message", fnsEx.Message } });

            await NotifyUserAsync("Email Error", "Email is not supported on this device.", "OK");
        }
        catch (Exception ex)
        {
            Analytics.TrackEvent("EmailError", new Dictionary<string, string>() { { "Message", ex.Message } });

            await NotifyUserAsync("Email Error", ex.Message, "OK");
        }
    }

    public void Initialize()
	{
        FlowDirection = TranslationService.FlowDirection;
        LoadingText = TranslationService.GetString("Loading", "Loading");
        PageTopic = TranslationService.GetString("Settings", "Settings");
        CreatedBy = TranslationService.GetString("CreatedBy", "Created by");
        DevelopedBy = TranslationService.GetString("DevelopedBy", "Developed by");
        DeveloperEmail = TranslationService.GetString("DeveloperEmail", "william@launchpaddevs.com");
        Launchpad = TranslationService.GetString("Launchpad", "Launchpad Developers");
    }

    [ObservableProperty]
    private string createdBy = string.Empty;

    [ObservableProperty]
    private string developedBy = string.Empty;

    [ObservableProperty]
    private string developerEmail = string.Empty;

    [ObservableProperty]
    private string launchpad = string.Empty;
}
