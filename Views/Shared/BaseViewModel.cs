using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpiritualGiftsSurvey.Helpers;
using SpiritualGiftsSurvey.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SpiritualGiftsSurvey.Views.Shared;

public abstract partial class BaseViewModel : ObservableObject, INotifyPropertyChanged
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

        IsTablet = DeviceInfo.Idiom == DeviceIdiom.Tablet;
    }

    #region INotifyPropertyChanging implementation

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

    [ObservableProperty]
    private FlowDirection flowDirection;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowControls))]
    private bool isLoading;

    [ObservableProperty]
    private string loadingText = string.Empty;

    [ObservableProperty]
    private string pageTopic = string.Empty;

    [ObservableProperty]
    private string navButtonText = string.Empty;

    [ObservableProperty]
    private string questionOf = string.Empty;

    [ObservableProperty]
    private string confirmButtonText = string.Empty;

    [ObservableProperty]
    private bool isTablet;

    [ObservableProperty]
    private bool showInstructable = Preferences.Default.Get(nameof(ShowInstructable), true);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowControls))]
    private bool errorVisible;

    public bool ShowControls
    {
        get { return !ErrorVisible && !IsLoading; }
    }

    partial void OnShowInstructableChanged(bool value)
    {
        Preferences.Default.Set(nameof(ShowInstructable), value);
    }

    [RelayCommand]
    private async Task OnNavButtonAsync(string page)
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

    public virtual Task InitAsync(INavigation nav)
    {
        throw new NotImplementedException("InitAsync must be implemented in derived ViewModels.");
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

    [RelayCommand]
    private void OnConfirm(string page)
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
