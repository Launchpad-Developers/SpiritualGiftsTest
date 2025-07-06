using SpiritualGiftsTest.Views.Shared;
using System.Runtime.Versioning;

namespace SpiritualGiftsTest.Views.Settings;

public partial class SettingsPage : BasePage
{
    public SettingsPage(SettingsViewModel vm)
        : base(vm)
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (ViewModel.FlowDirection == FlowDirection.RightToLeft)
        {
            BackArrow.IsVisible = false;
            BackArrow.IsEnabled = false;
            BackArrowRight.IsVisible = true;
            BackArrowRight.IsEnabled = true;
        }
    }

    private void StudentLanguage_Tapped(object sender, System.EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => {
            LanguagePicker.Focus();
        });
    }

    private void TeacherLanguage_Tapped(object sender, System.EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => {
            ParallelLanguagePicker.Focus();
        });
    }
}
