using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SpiritualGiftsTest.Messages;
using SpiritualGiftsTest.Models;
using SpiritualGiftsTest.Services;
using SpiritualGiftsTest.Views.Shared;
using System.Collections.ObjectModel;

namespace SpiritualGiftsTest.Views.Welcome;


public partial class WelcomeViewModel : BaseViewModel
{
    private bool _databaseOK;

    public WelcomeViewModel(
        IAggregatedServices aggregatedServices, 
        IPreferences preferences) : base(aggregatedServices, preferences)
    {
        Languages = new ObservableCollection<string>();

        Title = "Welcome to the Spiritual Gifts Test";

        WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (r, m) =>
        {
            _databaseOK = false;
            InitializePageAsync();
        });

        //Question = new Tuple<string, string>(string.Empty, string.Empty);
        Languages = new ObservableCollection<string>();

        InitializePageAsync();
    }

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private string tapTo = string.Empty;

    [ObservableProperty]
    private string navigate = string.Empty;

    [ObservableProperty]
    private string nextPage = string.Empty;

    [ObservableProperty]
    private QuestionViewModel question = new();

    [ObservableProperty]
    private ObservableCollection<string> languages = new();

    [RelayCommand]
    private void GetStarted()
    {
        // TODO: navigate to your next page, e.g.:
        // await Application.Current.MainPage.Navigation.PushAsync(
        //     new ContentPage() { Title = "Next" });
    }

    [RelayCommand]
    private async Task OpenInfoAsync()
    {
        if (!IsLoading)
            await NavigationService.NavigateAsync("AppInfo");
    }

    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        if (ErrorVisible)
        {
            await NotifyUserAsync("Network Error", "Your device is not connected to the internet. Language updates are not currently available.", "OK");
            return;
        }

        if (!IsLoading)
        {
            var result = await NavigationService.NavigateAsync("Settings");
            return;
        }
    }

    [RelayCommand]
    private void RetryUpdate()
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
            //VerseSource = new Tuple<string, string>(lang.Acts412, lang.Acts412v);
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
