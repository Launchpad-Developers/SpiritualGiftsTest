using SpiritualGiftsSurvey.Utilities;
using SpiritualGiftsSurvey.Views.Shared;

namespace SpiritualGiftsSurvey.Views.Survey;

public partial class SurveyPage : BasePage
{
    public SurveyPage(SurveyViewModel vm)
        : base(vm)
    {
        InitializeComponent();
        loadingBackground.IsVisible = true;
        loadingMessage.IsVisible = true;
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

            LeftButton.Source = (ImageSource)Application.Current!.Resources["NavRight"];
            RightButton.Source = (ImageSource)Application.Current!.Resources["NavLeft"];
        }

        var height = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
        var myHeight = DeviceInfo.Platform == DevicePlatform.iOS ? height - AppConstants.HeaderOffset_iOS : height - AppConstants.HeaderHeight;

        //ViewModel.ContentViews = new Dictionary<int, ContentView>()
        //{

        //};

        //MainLayout.Children.Add(ViewModel.ContentViews[1]);
        SetupNavigation(1);

        MainLayout.Children.Remove(NavLayout);
        MainLayout.Children.Add(NavLayout);
        NavLayout.IsVisible = true;

        loadingBackground.IsVisible = false;
        loadingMessage.IsVisible = false;
    }

    

    private void SetupNavigation(int currentPage)
    {
        //ViewModel.PageNumber = currentPage;

        //if (ViewModel.FlowDirection == FlowDirection.RightToLeft)
        //{
        //    LeftButton.TargetPageNumber = ViewModel.PageNumber + 1;
        //    LeftButton.CurrentContentView = RightButton.CurrentContentView = ViewModel.ContentViews[ViewModel.PageNumber];
        //    RightButton.TargetPageNumber = ViewModel.PageNumber - 1;

        //    if (ViewModel.PageNumber == 1)
        //        RightButton.IsVisible = false;
        //    else if (ViewModel.PageNumber == 20)
        //        LeftButton.IsVisible = false;
        //    else
        //        LeftButton.IsVisible = RightButton.IsVisible = true;

        //    LeftButton.TargetContentView = ViewModel.ContentViews.ContainsKey(ViewModel.PageNumber + 1) ? ViewModel.ContentViews[ViewModel.PageNumber + 1] : null;
        //    LeftButton.TargetPageNumber = ViewModel.PageNumber + 1;

        //    RightButton.TargetContentView = ViewModel.ContentViews.ContainsKey(ViewModel.PageNumber - 1) ? ViewModel.ContentViews[ViewModel.PageNumber - 1] : null;
        //    RightButton.TargetPageNumber = ViewModel.PageNumber - 1;
        //}
        //else
        //{
        //    RightButton.TargetPageNumber = ViewModel.PageNumber + 1;
        //    RightButton.CurrentContentView = LeftButton.CurrentContentView = ViewModel.ContentViews[ViewModel.PageNumber];
        //    LeftButton.TargetPageNumber = ViewModel.PageNumber - 1;

        //    if (ViewModel.PageNumber == 1)
        //        LeftButton.IsVisible = false;
        //    else if (ViewModel.PageNumber == 20)
        //        RightButton.IsVisible = false;
        //    else
        //        LeftButton.IsVisible = RightButton.IsVisible = true;

        //    RightButton.TargetContentView = ViewModel.ContentViews.ContainsKey(ViewModel.PageNumber + 1) ? ViewModel.ContentViews[ViewModel.PageNumber + 1] : null;
        //    RightButton.TargetPageNumber = ViewModel.PageNumber + 1;

        //    LeftButton.TargetContentView = ViewModel.ContentViews.ContainsKey(ViewModel.PageNumber - 1) ? ViewModel.ContentViews[ViewModel.PageNumber - 1] : null;
        //    LeftButton.TargetPageNumber = ViewModel.PageNumber - 1;
        //}
    }
}
