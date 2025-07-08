using SpiritualGiftsSurvey.Views.Shared;

namespace SpiritualGiftsSurvey.Views.AppInfo;

public partial class AppInfoPage : BasePage
{
    public AppInfoPage(AppInfoViewModel vm) : base(vm)
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
}
