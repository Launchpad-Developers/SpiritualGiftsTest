using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SpiritualGiftsSurvey.Helpers;
using SpiritualGiftsSurvey.Messages;
using SpiritualGiftsSurvey.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SpiritualGiftsSurvey.Views.Shared;

public abstract partial class BaseViewModel : ObservableObject, INotifyPropertyChanged
{

    protected readonly IAggregatedServices AggregatedServices;

    protected IDatabaseService DatabaseService => AggregatedServices.DatabaseService;
    protected ITranslationService TranslationService => AggregatedServices.TranslationService;
    protected IUrlService URLService => AggregatedServices.URLService;
    protected IDeviceStorageService DeviceStorageService => AggregatedServices.DeviceStorageService;
    protected INavigationService NavigationService => AggregatedServices.NavigationService;
    protected IAnalyticsService Analytics => AggregatedServices.AnalyticsService;
    protected IAppInfoService AppInfoService => AggregatedServices.AppInfoService;
    protected IEmailService EmailService => AggregatedServices.EmailService;
    protected ISurveyProgressService SurveyProgressService => AggregatedServices.SurveyProgressService;

    readonly IPreferences Prefs;

    // HIGH-2/HIGH-3/HIGH-4 FIX: Lifecycle coordination
    private Task? _currentInitTask;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public BaseViewModel(
        IAggregatedServices aggregatedServices,
        IPreferences prefs)
    {
        AggregatedServices = aggregatedServices;
        Prefs = prefs;

        IsTablet = DeviceInfo.Idiom == DeviceIdiom.Tablet;

        // HIGH-4 FIX: Message handler must await and handle exceptions
        WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (r, m) =>
        {
            // Dispatch async work with exception handling (prevent overlapping InitAsync)
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await InitializeAsync();
                }
                catch (Exception ex)
                {
                    Analytics.TrackEvent("LanguageChangeInitFailure",
                        new Dictionary<string, string>
                        {
                            { "Error", ex.Message }
                        });
                }
            });
        });
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
    [NotifyPropertyChangedFor(nameof(ShowControls))]
    [NotifyPropertyChangedFor(nameof(ShowNavButton))]
    private bool isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowControls))]
    private bool errorVisible;

    [ObservableProperty] private FlowDirection flowDirection;
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string loadingText = string.Empty;
    [ObservableProperty] private string pageTopic = string.Empty;
    [ObservableProperty] private string questionOf = string.Empty;
    [ObservableProperty] private string nextPageParameter = string.Empty;
    [ObservableProperty] private bool isTablet;
    [ObservableProperty] private bool showInstructable;

    public bool ShowNavButton => !IsLoading;

    protected bool RequiresInitialzation { get; set; } = true;

    public bool ShowControls
    {
        get { return !ErrorVisible && !IsLoading; }
    }

    partial void OnShowInstructableChanged(bool value)
    {
        Preferences.Default.Set(nameof(ShowInstructable), value);
    }

    [RelayCommand]
    private async Task OnNavButtonAsync(string route)
    {
        if (IsLoading) return;

        if (!string.IsNullOrEmpty(route))
        {
            if (Prefs.Get(nameof(ShowInstructable), true))
                ShowInstructable = true;
            else
                await PerformNavigation(route);
        }
        else
        {
            await NavBack(route);
        }
    }

    public abstract Task InitAsync();
    public abstract void RefreshViewModel();

    /// <summary>
    /// HIGH-2/HIGH-3/HIGH-4 FIX: Coordinated initialization preventing overlapping execution.
    /// Called by BasePage.OnNavigatedTo and LanguageChangedMessage handler.
    /// Ensures only one InitAsync runs at a time per ViewModel instance.
    /// </summary>
    public async Task InitializeAsync()
    {
        // If initialization is already running, await the existing task
        if (_currentInitTask != null && !_currentInitTask.IsCompleted)
        {
            await _currentInitTask;
            return;
        }

        // Acquire lock to prevent race between checking and starting init
        await _initLock.WaitAsync();
        try
        {
            // Double-check pattern: another caller might have started init while we waited
            if (_currentInitTask != null && !_currentInitTask.IsCompleted)
            {
                await _currentInitTask;
                return;
            }

            // Start new initialization and track the task
            _currentInitTask = InitAsync();
            await _currentInitTask;
        }
        finally
        {
            _initLock.Release();
        }
    }



    [RelayCommand]
    protected virtual async Task NavBack(string route)
    {
        try
        {
            await NavigationService.GoBackAsync(route);
        }
        catch (Exception ex)
        {
            Analytics.TrackEvent("NavBackFailure", new Dictionary<string, string>() { { "Message", ex.Message } });
        }
    }

    protected async Task PerformNavigation(string page, bool showLoading = true)
    {
        IsLoading = showLoading;

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

    protected static Task NotifyUserAsync(string title, string message, string ack)
        => PageHelper.ShowAlert(title, message, ack);

    protected static Task<bool> ConfirmUserAsync(string title, string message, string accept, string cancel)
        => PageHelper.ShowConfirm(title, message, accept, cancel);

    protected async Task RunWithLoading(Func<Task> action)
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;
            await Task.Yield(); // ensures overlay appears
            await action();
        }
        finally
        {
            IsLoading = false;
        }
    }
}
