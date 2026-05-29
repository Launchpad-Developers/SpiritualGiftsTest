using SpiritualGiftsSurvey.Routing;
using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Views.Shared;

namespace SpiritualGiftsSurvey.Views.Splash;

public partial class SplashPage : BasePage
{
    private readonly INavigationService _navigationService;
    private readonly IAnalyticsService _analyticsService;

    public SplashPage(
        SplashViewModel splashViewModel, 
        INavigationService navigationService,
        IAnalyticsService analyticsService)
        : base(splashViewModel)
    {
        _navigationService = navigationService;
        _analyticsService = analyticsService;
        BindingContext = splashViewModel;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // CRITICAL-3 FIX: Wrap async work in try/catch to ensure exceptions are observed
        // async void is permitted ONLY for framework override event handlers
        try
        {
            await PerformSplashSequenceAsync();
        }
        catch (Exception ex)
        {
            // Log error for diagnostics
            _analyticsService.TrackEvent("SplashNavigationFailure", 
                new Dictionary<string, string> 
                { 
                    { "Error", ex.Message },
                    { "StackTrace", ex.StackTrace ?? "none" }
                });
            
            // Navigate to welcome anyway (graceful degradation)
            // If navigation fails, user will see splash indefinitely (recoverable via app restart)
            try
            {
                await _navigationService.NavigateAsync(Routes.WelcomePage);
            }
            catch
            {
                // Suppress secondary navigation failure (nothing more we can do)
            }
        }
    }

    private async Task PerformSplashSequenceAsync()
    {
        // 4-second splash delay
        await Task.Delay(4000);
        
        // Navigate to welcome
        await _navigationService.NavigateAsync(Routes.WelcomePage);
    }
}