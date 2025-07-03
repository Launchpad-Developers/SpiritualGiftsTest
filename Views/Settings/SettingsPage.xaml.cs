using SpiritualGiftsTest.ViewModels;

namespace SpiritualGiftsTest.Views.Settings
{
    public partial class SettingsPage : ContentPage
    {
        private SettingsViewModel ViewModel;
        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ViewModel = (SettingsViewModel)BindingContext;

            if (ViewModel.FlowDirection == FlowDirection.RightToLeft)
            {
                BackArrow.IsVisible = false;
                BackArrow.IsEnabled = false;
                BackArrowRight.IsVisible = true;
                BackArrowRight.IsEnabled = true;
            }
        }

        private void PrimaryLanguageTapped(object sender, System.EventArgs e)
        {
            //BeginInvokeOnMainThread is necessary to ensure picker 
            //ALWAYS receives focus and UI displays the picklist
            MainThread.BeginInvokeOnMainThread(() => {
                PrimaryLanguagePicker.Focus();
            });
        }

        private void ParallelLanguageTapped(object sender, System.EventArgs e)
        {
            //BeginInvokeOnMainThread is necessary to ensure picker 
            //ALWAYS receives focus and UI displays the picklist
            MainThread.BeginInvokeOnMainThread(() => {
                ParallelLanguagePicker.Focus();
            });
        }
    }
}
