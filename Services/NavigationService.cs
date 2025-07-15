using System.Diagnostics;

namespace SpiritualGiftsSurvey.Services;

public interface INavigationService
{
    Task<bool> NavigateAsync(string route);
    Task<bool> NavigateAsync(string route, IDictionary<string, object> parameters);
    Task<bool> GoBackAsync(string route);
    Task<bool> GoBackAsync(string route, IDictionary<string, object> parameters);
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

    public async Task<bool> NavigateAsync(string route, IDictionary<string, object> parameters)
    {
        try
        {
            await Shell.Current.GoToAsync($"///{route}", true, parameters);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Navigation with parameters failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> GoBackAsync(string route)
    {
        try
        {
            await Shell.Current.GoToAsync($"//{route}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GoBack failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> GoBackAsync(string route, IDictionary<string, object> parameters)
    {
        try
        {
            await Shell.Current.GoToAsync($"//{route}", true, parameters);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GoBack failed: {ex.Message}");
            return false;
        }
    }
}
