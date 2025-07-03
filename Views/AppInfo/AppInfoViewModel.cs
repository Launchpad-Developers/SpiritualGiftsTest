using SpiritualGiftsTest.Interfaces;
using SpiritualGiftsTest.Views.Shared;
using System.Windows.Input;

namespace SpiritualGiftsTest.Views.AppInfo;

public class AppInfoViewModel : BaseViewModel
{
    public AppInfoViewModel(IAggregatedServices aggregatedServices) : base(aggregatedServices)
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
                Subject = "Only One Name",
                Body = "Information for Only One Name Bible study.",
                To = new List<string>() { emailAddress }
            };
            await Email.ComposeAsync(message);
        }
        catch (FeatureNotSupportedException fnsEx)
        {
            Analytics.TrackEvent("EmailNotSupported", new Dictionary<string, string>() { { "Message", fnsEx.Message } });
            await Application.Current.MainPage.DisplayAlert("Email Error", "Email is not supported on this device.", "OK");
        }
        catch (Exception ex)
        {
            Analytics.TrackEvent("EmailError", new Dictionary<string, string>() { { "Message", ex.Message } });
            await Application.Current.MainPage.DisplayAlert("Email Error", ex.Message, "OK");
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
            AppSmiths = lang.AppsmithsLLC;
            FlowDirection = lang.LanguageTextDirection.Equals("RL") ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
		}
    }

    private string _createdBy;
    public string CreatedBy
    {
        get => _createdBy;
        set { _createdBy = value; OnPropertyChanged(nameof(CreatedBy)); }
    }

    private string _revLong;
    public string RevLong
    {
        get => _revLong;
        set { _revLong = value; OnPropertyChanged(nameof(RevLong)); }
    }

    private string _revLongEmail;
    public string RevLongEmail
    {
        get => _revLongEmail;
        set { _revLongEmail = value; OnPropertyChanged(nameof(RevLongEmail)); }
    }

    private string _developedBy;
    public string DevelopedBy
    {
        get => _developedBy;
        set { _developedBy = value; OnPropertyChanged(nameof(DevelopedBy)); }
    }

    private string _revSmith;
    public string RevSmith
    {
        get => _revSmith;
        set { _revSmith = value; OnPropertyChanged(nameof(RevSmith)); }
    }

    private string _revSmithEmail;
    public string RevSmithEmail
    {
        get => _revSmithEmail;
        set { _revSmithEmail = value; OnPropertyChanged(nameof(RevSmithEmail)); }
    }

    private string _appsmiths;
    public string AppSmiths
    {
        get => _appsmiths;
        set { _appsmiths = value; OnPropertyChanged(nameof(AppSmiths)); }
    }
}
