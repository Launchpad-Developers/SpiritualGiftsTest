using System.Diagnostics;

namespace SpiritualGiftsSurvey.Services;

public interface INavigationService
{
    Task<bool> NavigateAsync(string route);
    Task GoBackToRootAsync();
}

public class NavigationService : INavigationService
{
    public async Task<bool> NavigateAsync(string route)
    {
        try
        {
            await Shell.Current.GoToAsync($"///{route}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Navigation failed: {ex.Message}");
            return false;
        }
    }

    public async Task GoBackToRootAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("//WelcomePage");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GoBackToRoot failed: {ex.Message}");
        }
    }
}
