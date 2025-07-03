using SpiritualGiftsTest.Interfaces;
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

    private static ISettings AppSettings => CrossSettings.Current;
    public BaseViewModel(IAggregatedServices aggregatedServices)
    {
        AggregatedServices = aggregatedServices;
        NavButtonCommand = new Command<string>(OnNavButtonCommand);
        ConfirmCommand = new Command<string>(OnConfirmCommand);

        IsTablet = DeviceInfo.Idiom == DeviceIdiom.Tablet;
    }

    public ICommand NavButtonCommand { get; }
    public ICommand ConfirmCommand { get; }

    private FlowDirection _flowDirection;
    public FlowDirection FlowDirection
    {
        get { return _flowDirection; }
        set { _flowDirection = value; OnPropertyChanged(nameof(FlowDirection)); }
    }

    private string _title;
    public string Title
    {
        get { return _title; }
        set { _title = value; OnPropertyChanged(nameof(Title)); }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get { return _isLoading; }
        set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
    }

    private string _loadingText;
    public string LoadingText
    {
        get { return _loadingText; }
        set { _loadingText = value; OnPropertyChanged(nameof(LoadingText)); }
    }

    public bool IsParallel
    {
        get => AppSettings.GetValueOrDefault(nameof(IsParallel), false);
        set { AppSettings.AddOrUpdateValue(nameof(IsParallel), value); OnPropertyChanged(nameof(IsParallel)); }
    }

    private string _pageTopic;
    public string PageTopic
    {
        get { return _pageTopic; }
        set { _pageTopic = value; OnPropertyChanged(nameof(PageTopic)); }
    }

    private string _navButtonText;
    public string NavButtonText
    {
        get { return _navButtonText; }
        set { _navButtonText = value; OnPropertyChanged(nameof(NavButtonText)); }
    }

    private string _pageOf;
    public string PageOf
    {
        get { return _pageOf; }
        set { _pageOf = value; OnPropertyChanged(nameof(PageOf)); }
    }

    private string _confirmButtonText;
    public string ConfirmButtonText
    {
        get { return _confirmButtonText; }
        set { _confirmButtonText = value; OnPropertyChanged(nameof(ConfirmButtonText)); }
    }

    private bool _isTablet;
    public bool IsTablet
    {
        get { return _isTablet; }
        set { _isTablet = value; OnPropertyChanged(nameof(IsTablet)); }
    }

    private bool _showInstructable;


    #region INotifyPropertyChanging implementation

    public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanging(string propertyName)
    {
        if (PropertyChanging == null)
            return;

        PropertyChanging(this, new System.ComponentModel.PropertyChangingEventArgs(propertyName));
    }
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    //public void OnPropertyChanged(string propertyName)
    //{
    //    if (PropertyChanged == null)
    //        return;

    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //}


    protected bool SetProperty<T>(ref T backingStore, T value,
        [CallerMemberName] string propertyName = "",
        Action onChanged = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;
        backingStore = value;
        onChanged?.Invoke();
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion

    public bool ShowInstructable
    {
        get { return _showInstructable; }
        set
        {
            AppSettings.AddOrUpdateValue(nameof(ShowInstructable), value);
            _showInstructable = value;
            OnPropertyChanged(nameof(ShowInstructable));
        }
    }

    private async void OnNavButtonCommand(string page)
    {
        if (IsLoading) return;

        if (!string.IsNullOrEmpty(page))
        {
            if (AppSettings.GetValueOrDefault(nameof(ShowInstructable), true))
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

    protected async void NotifyUserAsync(string title, string message, string ack)
    {
        await Application.Current.MainPage.DisplayAlert(title, message, ack);
    }
}
