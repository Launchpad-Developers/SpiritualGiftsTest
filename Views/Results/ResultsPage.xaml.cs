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
        base.OnNavigatedTo(args);

        ViewModel.InitAsync();
    }
}