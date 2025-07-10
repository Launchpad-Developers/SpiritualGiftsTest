using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpiritualGiftsSurvey.Helpers;
using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Views.Shared;
using System.Runtime.Versioning;

namespace SpiritualGiftsSurvey.Views.AppInfo;

[SupportedOSPlatform("android")]
[SupportedOSPlatform("ios")]
public partial class AppInfoViewModel : BaseViewModel
{

    private string _developerWebsiteUrl = "https://launchpaddevs.com";
    private string _cornerstoneWebsiteUrl = "https://www.cornerstoneupc.com/";

    public AppInfoViewModel(
        IAggregatedServices aggregatedServices,
        IPreferences preferences) : base(aggregatedServices, preferences)
    {

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
            Analytics.TrackEvent("EmailNotSupported", new Dictionary<string, string>
            {
                { "Message", fnsEx.Message }
            });

            await NotifyUserAsync("Email Error", "Email is not supported on this device.", "OK");
        }
        catch (Exception ex)
        {
            Analytics.TrackEvent("EmailError", new Dictionary<string, string>
            {
                { "Message", ex.Message }
            });

            await NotifyUserAsync("Email Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task OpenWebsiteAsync(string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                await NotifyUserAsync("URL Error", "No website URL was provided.", "OK");
                return;
            }

            Uri? uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                await NotifyUserAsync("URL Error", $"The URL '{url}' is invalid.", "OK");
                return;
            }

            await Launcher.Default.OpenAsync(uri);
        }
        catch (FeatureNotSupportedException fnsEx)
        {
            Analytics.TrackEvent("BrowserNotSupported", new Dictionary<string, string>
            {
                { "Message", fnsEx.Message }
            });

            await NotifyUserAsync("Browser Error", "Opening a web browser is not supported on this device.", "OK");
        }
        catch (Exception ex)
        {
            Analytics.TrackEvent("OpenWebsiteError", new Dictionary<string, string>
            {
                { "Message", ex.Message }
            });

            await NotifyUserAsync("Browser Error", ex.Message, "OK");
        }
    }

    public override void InitAsync()
	{
        if (!RequiresInitialzation)
            return;

        RequiresInitialzation = false;

        FlowDirection = TranslationService.FlowDirection;

        PageTopic = TranslationService.GetString("AppInfo", "App Info");
        CreatedBy = TranslationService.GetString("CreatedBy", "Created By");
        CreatedByDetail = TranslationService.GetString("CreatedByDetail", "Based on the Wagner Modified Houts Questionnaire");
        DevelopedFor = TranslationService.GetString("DevelopedFor", "Developed For");
        CornerstoneUpc = TranslationService.GetString("CornerstoneUpc", "Cornerstone UPC");
        CornerstoneWebsite = TranslationService.GetString("CornerstoneWebsite", _cornerstoneWebsiteUrl);
        DevelopedBy = TranslationService.GetString("DevelopedBy", "Developed By");
        DeveloperName = TranslationService.GetString("DeveloperName", "William Smith");
        DeveloperEmail = TranslationService.GetString("DeveloperEmail", "william@launchpaddevs.com");
        Launchpad = TranslationService.GetString("Launchpad", "Launchpad Developers");
        CompanyWebsite = TranslationService.GetString("CompanyWebsite", _developerWebsiteUrl);
        AppVersionLabel = TranslationService.GetString("AppVersionLabel", "App Version");
        AppVersion = $"v {AppInfoService.GetVersionString()}.0";
        DatabaseLabel = TranslationService.GetString("DatabaseLabel", "Database Info");

        var dbInfo = DatabaseService.GetDatabaseInfo();

        DatabaseVersion = $"Build {dbInfo?.Version.ToString()}" ?? "Build: Unknown";

        var formatted = PageHelper.FormatFlatDate(dbInfo?.Date);
        DatabaseDate = $"Last updated {formatted}";
    }

    [ObservableProperty]
    private string createdBy = string.Empty;

    [ObservableProperty]
    private string createdByDetail = string.Empty;

    [ObservableProperty]
    private string developedFor = string.Empty;

    [ObservableProperty]
    private string cornerstoneUpc = string.Empty;

    [ObservableProperty]
    private string cornerstoneWebsite = string.Empty;

    [ObservableProperty]
    private string developedBy = string.Empty;

    [ObservableProperty]
    private string developerName = string.Empty;

    [ObservableProperty]
    private string developerEmail = string.Empty;

    [ObservableProperty]
    private string launchpad = string.Empty;

    [ObservableProperty]
    private string companyWebsite = string.Empty;

    [ObservableProperty]
    private string appVersionLabel = string.Empty;

    [ObservableProperty]
    private string appVersion = string.Empty;

    [ObservableProperty]
    private string databaseLabel = string.Empty;

    [ObservableProperty]
    private string databaseVersion = string.Empty;

    [ObservableProperty]
    private string databaseDate = string.Empty;
}
