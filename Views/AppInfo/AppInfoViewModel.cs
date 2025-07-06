using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpiritualGiftsTest.Services;
using SpiritualGiftsTest.Views.Shared;
using System.Runtime.Versioning;
using System.Windows.Input;

namespace SpiritualGiftsTest.Views.AppInfo;

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
                Subject = "Spiritual Gifts Test",
                Body = "Information for Spiritual Gifts Test.",
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
        var lang = TranslationService.PrimaryLanguage;

			if (lang != null)
			{
                PageTopic = lang.SpiritualGiftsTestTitle;
                CreatedBy = lang.CreatedBy;
                RevLong = lang.RevLong;
                RevLongEmail = lang.RevLongEmail;
                DevelopedBy = lang.DevelopedBy;
                RevSmith = lang.RevSmith;
                RevSmithEmail = lang.RevSmithEmail;
                Launchpad = lang.AppsmithsLLC;
                FlowDirection = lang.LanguageTextDirection.Equals("RL") ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
		    }
    }

    [ObservableProperty]
    private string createdBy = string.Empty;

    [ObservableProperty]
    private string revLong = string.Empty;

    [ObservableProperty]
    private string revLongEmail = string.Empty;

    [ObservableProperty]
    private string developedBy = string.Empty;

    [ObservableProperty]
    private string revSmith = string.Empty;

    [ObservableProperty]
    private string revSmithEmail = string.Empty;

    [ObservableProperty]
    private string launchpad = string.Empty;
}
