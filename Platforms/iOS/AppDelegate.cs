using Foundation;
using UIKit;

namespace SpiritualGiftsSurvey;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication app, NSDictionary options)
    {
        // Hide status bar
        UIApplication.SharedApplication.StatusBarHidden = true;

        return base.FinishedLaunching(app, options);
    }

}
