namespace SpiritualGiftsSurvey.Services;

public interface IAppInfoService
{
    string GetVersionString();
    string GetBuildString();
}

public class AppInfoService : IAppInfoService
{
    public string GetVersionString() => AppInfo.Current.VersionString;
    public string GetBuildString() => AppInfo.Current.BuildString;
}

