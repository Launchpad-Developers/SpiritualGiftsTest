using CommunityToolkit.Mvvm.Messaging;
using SpiritualGiftsSurvey.Messages;
using SpiritualGiftsSurvey.Views.Shared;

namespace SpiritualGiftsSurvey.Views.Survey;

public partial class SurveyPage : BasePage
{
    public SurveyPage(SurveyViewModel vm)
        : base(vm)
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<ScrollToQuestionMessage>(this, async (r, m) =>
        {
            // ScrollView doesn't have ScrollTo by index, so we need to calculate position
            // For now, scroll to top - could enhance later if needed
            await QuestionsScrollView.ScrollToAsync(0, 0, animated: true);
        });

    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        // Reset scroll position to top
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await QuestionsScrollView.ScrollToAsync(0, 0, animated: false);
        });
        base.OnNavigatedTo(args);
    }
}
