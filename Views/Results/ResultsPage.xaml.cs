using SpiritualGiftsSurvey.Views.Shared;

namespace SpiritualGiftsSurvey.Views.Results;

public partial class ResultsPage : BasePage
{
    public ResultsPage(ResultsViewModel vm) : base(vm)
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        ResultsCollectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
        base.OnNavigatedTo(args);
    }
}