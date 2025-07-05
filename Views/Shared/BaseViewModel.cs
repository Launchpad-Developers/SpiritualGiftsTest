using SpiritualGiftsTest.Helpers;
using SpiritualGiftsTest.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SpiritualGiftsTest.Views.Shared;

public abstract class BaseViewModel : INotifyPropertyChanged
{

    protected readonly IAggregatedServices AggregatedServices;

    protected IDatabaseService DatabaseService => AggregatedServices.DatabaseService;
    protected ITranslationService TranslationService => AggregatedServices.TranslationService;
    protected IURLService URLService => AggregatedServices.URLService;
    protected IDeviceStorageService DeviceStorageService => AggregatedServices.DeviceStorageService;
    protected INavigationService NavigationService => AggregatedServices.NavigationService;
    protected IAnalyticsService Analytics => AggregatedServices.AnalyticsService;

    readonly IPreferences Prefs;

    public BaseViewModel(
        IAggregatedServices aggregatedServices,
        IPreferences prefs)
    {
        AggregatedServices = aggregatedServices;
        Prefs = prefs;
        NavButtonCommand = new Command<string>(OnNavButtonCommand);
        ConfirmCommand = new Command<string>(OnConfirmCommand);

        IsTablet = DeviceInfo.Idiom == DeviceIdiom.Tablet;
    }

    public ICommand NavButtonCommand { get; }
    public ICommand ConfirmCommand { get; }


    #region INotifyPropertyChanging implementation

    public event System.ComponentModel.PropertyChangingEventHandler? PropertyChanging;
    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanging(string propertyName)
    {
        if (PropertyChanging == null)
            return;

        PropertyChanging(this, new System.ComponentModel.PropertyChangingEventArgs(propertyName));
    }
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


    protected bool SetProperty<T>(ref T backingStore, T value,
        [CallerMemberName] string propertyName = "",
        Action? onChanged = null) // Change Action to nullable type Action?  
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;
        backingStore = value;
        onChanged?.Invoke();
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion

    private FlowDirection _flowDirection;
    public FlowDirection FlowDirection
    {
        get => _flowDirection;
        set => SetProperty(ref _flowDirection, value);
    }

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string _loadingText = string.Empty;
    public string LoadingText
    {
        get => _loadingText;
        set => SetProperty(ref _loadingText, value);
    }

    private string _pageTopic = string.Empty;
    public string PageTopic
    {
        get => _pageTopic;
        set => SetProperty(ref _pageTopic, value);
    }

    private string _navButtonText = string.Empty;
    public string NavButtonText
    {
        get => _navButtonText;
        set => SetProperty(ref _navButtonText, value);
    }

    private string _pageOf = string.Empty;
    public string PageOf
    {
        get => _pageOf;
        set => SetProperty(ref _pageOf, value);
    }

    private string _confirmButtonText = string.Empty;
    public string ConfirmButtonText
    {
        get => _confirmButtonText;
        set => SetProperty(ref _confirmButtonText, value);
    }

    private bool _isTablet;
    public bool IsTablet
    {
        get => _isTablet;
        set => SetProperty(ref _isTablet, value);
    }

    private bool _isParallel = Preferences.Default.Get(nameof(IsParallel), false);
    public bool IsParallel
    {
        get => _isParallel;
        set
        {
            if (SetProperty(ref _isParallel, value))
                Preferences.Default.Set(nameof(IsParallel), value);
        }
    }

    private bool _showInstructable = Preferences.Default.Get(nameof(ShowInstructable), true);
    public bool ShowInstructable
    {
        get => _showInstructable;
        set
        {
            if (SetProperty(ref _showInstructable, value))
                Preferences.Default.Set(nameof(ShowInstructable), value);
        }
    }

    private async void OnNavButtonCommand(string page)
    {
        if (IsLoading) return;

        if (!string.IsNullOrEmpty(page))
        {
            if (Prefs.Get(nameof(ShowInstructable), true))
                ShowInstructable = true;
            else
                PerformNavigation(page);
        }
        else
        {
            await NavBack();
        }
    }

    protected async Task NavBack()
    {
        try
        {
            await NavigationService.GoBackToRootAsync();
        }
        catch (Exception ex)
        {
            Analytics.TrackEvent("NavBackFailure", new Dictionary<string, string>() { { "Message", ex.Message } });
        }
    }

    private void OnConfirmCommand(string page)
    {
        //Set to false in settings after hitting "Got It" here
        ShowInstructable = false;
        PerformNavigation(page);
    }

    private async void PerformNavigation(string page)
    {
        IsLoading = true;

        try
        {
            var result = await NavigationService.NavigateAsync(page);
            //return;
        }
        catch (Exception ex)
        {
            Analytics.TrackEvent("NavFailure", new Dictionary<string, string>() { { "Message", ex.Message } });
        }

        IsLoading = false;
    }

    protected Task NotifyUserAsync(string title, string message, string ack)
        => PageHelper.ShowAlert(title, message, ack);

    protected Task<bool> ConfirmUserAsync(string title, string message, string accept, string cancel)
        => PageHelper.ShowConfirm(title, message, accept, cancel);
}
