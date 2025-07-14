using SpiritualGiftsSurvey.Views.Shared;

namespace SpiritualGiftsSurvey.Views.GiftDescription;

public partial class GiftDescriptionPage : BasePage
{
	public GiftDescriptionPage(GiftDescriptionViewModel viewModel)
        : base(viewModel)
	{
		InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        ViewModel.InitAsync();

        if (ViewModel.FlowDirection == FlowDirection.RightToLeft)
        {
            BackArrow.IsVisible = false;
            BackArrow.IsEnabled = false;
            BackArrowRight.IsVisible = true;
            BackArrowRight.IsEnabled = true;
        }
    }
}