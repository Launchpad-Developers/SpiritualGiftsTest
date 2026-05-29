using SpiritualGiftsSurvey.Views.Shared;
using System.Runtime.Versioning;

namespace SpiritualGiftsSurvey.Views.Settings;

public partial class SettingsPage : BasePage
{
    public SettingsPage(SettingsViewModel vm)
        : base(vm)
    {
        InitializeComponent();
    }

    private void StudentLanguage_Tapped(object sender, System.EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => {
            LanguagePicker.Focus();
            ((SettingsViewModel)ViewModel).ShowLanguagePicker = true;
        });
    }

    private void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        ((SettingsViewModel)ViewModel).ResetAddReportingEmailEntry();
    }
}
