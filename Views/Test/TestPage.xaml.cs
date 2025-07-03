using SpiritualGiftsTest.Controls;
using SpiritualGiftsTest.Controls.StudyContent;
using SpiritualGiftsTest.Helpers;
using SpiritualGiftsTest.Interfaces;
using SpiritualGiftsTest.ViewModels;

namespace SpiritualGiftsTest.Views
{
    public partial class TestPage : ContentPage
    {
        private TestViewModel ViewModel { get; set; }

        public TestPage()
        {
            InitializeComponent();
            loadingBackground.IsVisible = true;
            loadingMessage.IsVisible = true;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ViewModel = (TestViewModel)BindingContext;

            if (ViewModel.FlowDirection == FlowDirection.RightToLeft)
            {
                BackArrow.IsVisible = false;
                BackArrow.IsEnabled = false;
                BackArrowRight.IsVisible = true;
                BackArrowRight.IsEnabled = true;

                LeftButton.Source = (ImageSource)Application.Current.Resources["NavRight"];
                RightButton.Source = (ImageSource)Application.Current.Resources["NavLeft"];
            }

            var height = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
            var myHeight = DeviceInfo.Platform == DevicePlatform.iOS ? height - AppConstants.HeaderOffset_iOS : height - AppConstants.HeaderHeight;

            ViewModel.ContentViews = new Dictionary<int, ContentView>()
            {
                { 1, new StudyContent1(myHeight) { Opacity = 1, Model = ViewModel.PrimaryLanguageTranslation } },
                { 2, new StudyContent2(myHeight) { Opacity = 0, Model = ViewModel.PrimaryLanguageTranslation } },
                { 3, new StudyContent3(myHeight) { Opacity = 0, Model = ViewModel.PrimaryLanguageTranslation } },
                { 4, new StudyContent4(myHeight) { Opacity = 0, Model = ViewModel.PrimaryLanguageTranslation } },
                { 5, new StudyContent5(myHeight) { Opacity = 0, Model = ViewModel.PrimaryLanguageTranslation } },
                { 6, new StudyContent6(myHeight) { Opacity = 0, Model = ViewModel.PrimaryLanguageTranslation } },
                { 7, new StudyContent7(myHeight) { Opacity = 0, Model = ViewModel.PrimaryLanguageTranslation } },
                { 8, new StudyContent8(myHeight) { Opacity = 0, Model = ViewModel.PrimaryLanguageTranslation } },
                { 9, new StudyContent9(myHeight) { Opacity = 0, Model = ViewModel.PrimaryLanguageTranslation } },
                { 10, new StudyContent10(myHeight) { Opacity = 0, Model = ViewModel.PrimaryLanguageTranslation } },
                { 11, new StudyContent11(myHeight) { Opacity = 0, Model = ViewModel.PrimaryLanguageTranslation } },
                { 12, new StudyContent12(myHeight) { Opacity = 0, Model = ViewModel.PrimaryLanguageTranslation } },
                { 13, new StudyContent13(myHeight) { Opacity = 0, Model = ViewModel.PrimaryLanguageTranslation } },
                { 14, new StudyContent14(myHeight) { Opacity = 0, Model = ViewModel.PrimaryLanguageTranslation } },
                { 15, new StudyContent15(myHeight) { Opacity = 0, Model = ViewModel.PrimaryLanguageTranslation } },
                { 16, new StudyContent16(myHeight) { Opacity = 0, Model = ViewModel.PrimaryLanguageTranslation } },
                { 17, new StudyContent17(myHeight) { Opacity = 0, Model = ViewModel.PrimaryLanguageTranslation } },
                { 18, new StudyContent18(myHeight) { Opacity = 0, Model = ViewModel.PrimaryLanguageTranslation } },
                { 19, new StudyContent19(myHeight) { Opacity = 0, Model = ViewModel.PrimaryLanguageTranslation } },
                { 20, new StudyContent20(myHeight) { Opacity = 0, Model = ViewModel.PrimaryLanguageTranslation, Command = ViewModel.NavButtonCommand } }
            };

            MainLayout.Children.Add(ViewModel.ContentViews[1]);
            SetupNavigation(1);

            MainLayout.Children.Remove(NavLayout);
            MainLayout.Children.Add(NavLayout);
            NavLayout.IsVisible = true;

            loadingBackground.IsVisible = false;
            loadingMessage.IsVisible = false;
        }

        private async void ClickedLastPage(System.Object sender, System.EventArgs e)
        {
            var button = sender as ASImageButton;
            if (button.TargetContentView != null)
            {
                await button.CurrentContentView.FadeTo(0, 300);

                MainLayout.Children.Remove(button.CurrentContentView);

                ViewModel.NavigatedCommand?.Execute(new TestNavParameter { TargetPage = button.TargetPageNumber, TargetPageTopic = ((IPageTopicHolder)button.TargetContentView).PageTopic });

                MainLayout.Children.Add(button.TargetContentView);
                
                MainLayout.Children.Remove(NavLayout);
                MainLayout.Children.Add(NavLayout);

                await button.TargetContentView.FadeTo(1, 300);
            }
            SetupNavigation(ViewModel.PageNumber - 1);
        }

        private async void ClickedNextPage(System.Object sender, System.EventArgs e)
        {
            var button = sender as ASImageButton;
            if (button.TargetContentView != null)
            {
                await button.CurrentContentView.FadeTo(0, 300);

                MainLayout.Children.Remove(button.CurrentContentView);

                ViewModel.NavigatedCommand?.Execute(new TestNavParameter { TargetPage = button.TargetPageNumber, TargetPageTopic = ((IPageTopicHolder)button.TargetContentView).PageTopic });

                MainLayout.Children.Add(button.TargetContentView);

                MainLayout.Children.Remove(NavLayout);
                MainLayout.Children.Add(NavLayout);

                await button.TargetContentView.FadeTo(1, 300);
            }

            SetupNavigation(ViewModel.PageNumber + 1);
        }

        private void SetupNavigation(int currentPage)
        {
            ViewModel.PageNumber = currentPage;

            if (ViewModel.FlowDirection == FlowDirection.RightToLeft)
            {
                LeftButton.TargetPageNumber = ViewModel.PageNumber + 1;
                LeftButton.CurrentContentView = RightButton.CurrentContentView = ViewModel.ContentViews[ViewModel.PageNumber];
                RightButton.TargetPageNumber = ViewModel.PageNumber - 1;

                if (ViewModel.PageNumber == 1)
                    RightButton.IsVisible = false;
                else if (ViewModel.PageNumber == 20)
                    LeftButton.IsVisible = false;
                else
                    LeftButton.IsVisible = RightButton.IsVisible = true;

                LeftButton.TargetContentView = ViewModel.ContentViews.ContainsKey(ViewModel.PageNumber + 1) ? ViewModel.ContentViews[ViewModel.PageNumber + 1] : null;
                LeftButton.TargetPageNumber = ViewModel.PageNumber + 1;

                RightButton.TargetContentView = ViewModel.ContentViews.ContainsKey(ViewModel.PageNumber - 1) ? ViewModel.ContentViews[ViewModel.PageNumber - 1] : null;
                RightButton.TargetPageNumber = ViewModel.PageNumber - 1;
            }
            else
            {
                RightButton.TargetPageNumber = ViewModel.PageNumber + 1;
                RightButton.CurrentContentView = LeftButton.CurrentContentView = ViewModel.ContentViews[ViewModel.PageNumber];
                LeftButton.TargetPageNumber = ViewModel.PageNumber - 1;

                if (ViewModel.PageNumber == 1)
                    LeftButton.IsVisible = false;
                else if (ViewModel.PageNumber == 20)
                    RightButton.IsVisible = false;
                else
                    LeftButton.IsVisible = RightButton.IsVisible = true;

                RightButton.TargetContentView = ViewModel.ContentViews.ContainsKey(ViewModel.PageNumber + 1) ? ViewModel.ContentViews[ViewModel.PageNumber + 1] : null;
                RightButton.TargetPageNumber = ViewModel.PageNumber + 1;

                LeftButton.TargetContentView = ViewModel.ContentViews.ContainsKey(ViewModel.PageNumber - 1) ? ViewModel.ContentViews[ViewModel.PageNumber - 1] : null;
                LeftButton.TargetPageNumber = ViewModel.PageNumber - 1;
            }
        }
    }
}
