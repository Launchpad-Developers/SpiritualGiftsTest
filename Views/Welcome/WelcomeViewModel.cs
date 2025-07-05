using CommunityToolkit.Mvvm.Messaging;
using SpiritualGiftsTest.Messages;
using SpiritualGiftsTest.Services;
using SpiritualGiftsTest.Views.Shared;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SpiritualGiftsTest.Views.Welcome;

public class WelcomeViewModel : BaseViewModel
{
   


    public ICommand GetStartedCommand { get; }

    private void OnGetStarted()
    {
        // TODO: navigate to your next page, e.g.:
        // await Application.Current.MainPage.Navigation.PushAsync(
        //     new ContentPage() { Title = "Next" });
    }

    private bool _databaseOK;

    public WelcomeViewModel(
        IAggregatedServices aggregatedServices, 
        IPreferences preferences) : base(aggregatedServices, preferences)
    {
        OpenInfo = new Command(OnOpenInfo);
        OpenSettings = new Command(OnOpenSettings);
        RetryUpdate = new Command(OnRetryUpdate);

        Languages = new ObservableCollection<string>();

        Title = "Welcome to the Spiritual Gifts Test";

        WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (r, m) =>
        {
            _databaseOK = false;
            InitializePageAsync();
        });

        _verseSource = new Tuple<string, string>(string.Empty, string.Empty);
        _languages = new ObservableCollection<string>();

        GetStartedCommand = new Command(OnGetStarted);

        InitializePageAsync();
    }

    public ICommand OpenInfo { get; }
    public ICommand OpenSettings { get; }
    public ICommand RetryUpdate { get; }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get { return _errorMessage; }
        set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
    }

    private bool _errorVisible;
    public bool ErrorVisible
    {
        get { return _errorVisible; }
        set { _errorVisible = value; OnPropertyChanged(nameof(ErrorVisible)); OnPropertyChanged(nameof(ShowControls)); }
    }

    public bool ShowControls
    {
        get { return !ErrorVisible && !IsLoading; }
    }

    private string _tapTo = string.Empty;
    public string TapTo
    {
        get { return _tapTo; }
        set { _tapTo = value; OnPropertyChanged(nameof(TapTo)); }
    }

    private string _navigate = string.Empty;
    public string Navigate
    {
        get { return _navigate; }
        set { _navigate = value; OnPropertyChanged(nameof(Navigate)); }
    }

    private string _nextPage = string.Empty;
    public string NextPage
    {
        get { return _nextPage; }
        set { _nextPage = value; OnPropertyChanged(nameof(NextPage)); }
    }

    private Tuple<string, string> _verseSource;
    public Tuple<string, string> VerseSource
    {
        get { return _verseSource; }
        set { _verseSource = value; OnPropertyChanged(nameof(VerseSource)); }
    }

    private ObservableCollection<string> _languages;
    public ObservableCollection<string> Languages
    {
        get { return _languages; }
        set { _languages = value; OnPropertyChanged(nameof(Languages)); }
    }

    private async void OnOpenInfo()
    {
        if (!IsLoading)
            await NavigationService.NavigateAsync("AppInfo");
    }

    private async void OnOpenSettings()
    {
        if (ErrorVisible)
        {
            await NotifyUserAsync("Network Error", "Your device is not connected to the internet.  Language updates are not currently available.", "OK");
            return;
        }

        if (!IsLoading)
        {
            var result = await NavigationService.NavigateAsync("Settings");
            return;
        }
    }

    private void OnRetryUpdate()
    {
        InitializePageAsync();
    }

    public async void InitializePageAsync()
    {
        if (_databaseOK) return;

        LoadingText = "Loading";
        IsLoading = true;
        ErrorVisible = false;
        OnPropertyChanged(nameof(ShowControls));

        await TranslationService.InitializeLanguages();

        var lang = TranslationService.PrimaryLanguage;

        if (lang == null)
        {
            PageTopic = "Spiritual Gifts Test";
            ErrorMessage = "No Internet Connection \nNo Translations Available";
            NavButtonText = "Retry";
            ErrorVisible = true;
        }
        else
        {
            FlowDirection = lang.LanguageTextDirection.Equals("RL") ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            LoadingText = lang.Loading;
            PageTopic = lang.BibleStudyTitle;
            VerseSource = new Tuple<string, string>(lang.Acts412, lang.Acts412v);
            NavButtonText = lang.NavButtonBegin;
            _databaseOK = true;
            ConfirmButtonText = lang.GotIt;
            TapTo = lang.TapTo;
            Navigate = lang.Navigate;
        }

        NextPage = IsParallel ? "ParallelStudyPage" : "StudyPage";

        IsLoading = false;
        OnPropertyChanged(nameof(ShowControls));
    }

}
