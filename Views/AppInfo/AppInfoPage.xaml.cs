using System;
using System.Collections.Generic;

namespace SpiritualGiftsTest.Views.AppInfo;

public partial class AppInfoPage : ContentPage
{
    private AppInfoViewModel ViewModel { get; set; }

    public AppInfoPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ViewModel = (AppInfoViewModel)BindingContext;

        if (ViewModel.FlowDirection == FlowDirection.RightToLeft)
        {
            BackArrow.IsVisible = false;
            BackArrow.IsEnabled = false;
            BackArrowRight.IsVisible = true;
            BackArrowRight.IsEnabled = true;
        }
    }
}
