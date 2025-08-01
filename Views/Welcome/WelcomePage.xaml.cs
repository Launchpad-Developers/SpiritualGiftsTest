using Microsoft.Maui.Controls.PlatformConfiguration;
using SpiritualGiftsSurvey.Views.Shared;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace SpiritualGiftsSurvey.Views.Welcome;


public partial class WelcomePage : BasePage
{
    public WelcomePage(WelcomeViewModel vm) : base(vm)
    {
        InitializeComponent();

        On<iOS>().SetUseSafeArea(false);
    }
}