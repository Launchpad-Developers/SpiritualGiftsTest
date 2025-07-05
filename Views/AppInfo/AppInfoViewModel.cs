using SpiritualGiftsTest.Services;
using SpiritualGiftsTest.Views.Shared;
using System.Windows.Input;

namespace SpiritualGiftsTest.Views.AppInfo;

public class AppInfoViewModel : BaseViewModel
{
    public AppInfoViewModel(
        IAggregatedServices aggregatedServices,
        IPreferences preferences) : base(aggregatedServices, preferences)
    {
		EmailCommand = new Command<string>(OnEmailCommand);

        Initialize();
    }

	public ICommand EmailCommand { get; }
    
    private async void OnEmailCommand(string emailAddress)
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

    private string _createdBy = string.Empty;
    public string CreatedBy
    {
        get => _createdBy;
        set { _createdBy = value; OnPropertyChanged(nameof(CreatedBy)); }
    }

    private string _revLong = string.Empty;
    public string RevLong
    {
        get => _revLong;
        set { _revLong = value; OnPropertyChanged(nameof(RevLong)); }
    }

    private string _revLongEmail = string.Empty;
    public string RevLongEmail
    {
        get => _revLongEmail;
        set { _revLongEmail = value; OnPropertyChanged(nameof(RevLongEmail)); }
    }

    private string _developedBy = string.Empty;
    public string DevelopedBy
    {
        get => _developedBy;
        set { _developedBy = value; OnPropertyChanged(nameof(DevelopedBy)); }
    }

    private string _revSmith = string.Empty;
    public string RevSmith
    {
        get => _revSmith;
        set { _revSmith = value; OnPropertyChanged(nameof(RevSmith)); }
    }

    private string _revSmithEmail = string.Empty;
    public string RevSmithEmail
    {
        get => _revSmithEmail;
        set { _revSmithEmail = value; OnPropertyChanged(nameof(RevSmithEmail)); }
    }

    private string _launchpad = string.Empty;
    public string Launchpad
    {
        get => _launchpad;
        set { _launchpad = value; OnPropertyChanged(nameof(Launchpad)); }
    }
}
