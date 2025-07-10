using SpiritualGiftsSurvey.Views.Shared;

namespace SpiritualGiftsSurvey.Views.Welcome;


public partial class WelcomePage : BasePage
{
    public WelcomePage(WelcomeViewModel vm) : base(vm)
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ViewModel.InitAsync();
    }
}