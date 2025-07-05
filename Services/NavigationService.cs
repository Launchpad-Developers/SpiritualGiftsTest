namespace SpiritualGiftsTest.Services;

public interface INavigationService
{
    Task<bool> NavigateAsync(string route);
    Task GoBackToRootAsync();
}

public class NavigationService : INavigationService
{
    public Task GoBackToRootAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> NavigateAsync(string route)
    {
        throw new NotImplementedException();
    }
}
