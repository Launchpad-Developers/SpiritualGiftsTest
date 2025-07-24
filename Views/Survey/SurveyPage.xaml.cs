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

        WeakReferenceMessenger.Default.Register<ScrollToQuestionMessage>(this, (r, m) =>
        {
            QuestionsCollectionView.ScrollTo(m.Value, position: ScrollToPosition.Start, animate: true);
        });

    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        QuestionsCollectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
        base.OnNavigatedTo(args);
    }
}
