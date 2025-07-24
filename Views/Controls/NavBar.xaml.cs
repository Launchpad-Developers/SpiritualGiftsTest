using System.Windows.Input;
using Microsoft.Maui.Layouts;

namespace SpiritualGiftsSurvey.Views.Controls;

public partial class NavBar : ContentView
{
    public NavBar()
    {
        InitializeComponent();
        UpdateTitleAlignment(); // initial alignment
    }

    // Title
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(NavBar), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // Left icon source
    public static readonly BindableProperty LeftIconProperty =
        BindableProperty.Create(nameof(LeftIcon), typeof(ImageSource), typeof(NavBar));

    public ImageSource LeftIcon
    {
        get => (ImageSource)GetValue(LeftIconProperty);
        set => SetValue(LeftIconProperty, value);
    }

    // Right icon source
    public static readonly BindableProperty RightIconProperty =
        BindableProperty.Create(nameof(RightIcon), typeof(ImageSource), typeof(NavBar));

    public ImageSource RightIcon
    {
        get => (ImageSource)GetValue(RightIconProperty);
        set => SetValue(RightIconProperty, value);
    }

    // Left button command
    public static readonly BindableProperty LeftCommandProperty =
        BindableProperty.Create(nameof(LeftCommand), typeof(ICommand), typeof(NavBar));

    public ICommand LeftCommand
    {
        get => (ICommand)GetValue(LeftCommandProperty);
        set => SetValue(LeftCommandProperty, value);
    }

    // Right button command
    public static readonly BindableProperty RightCommandProperty =
        BindableProperty.Create(nameof(RightCommand), typeof(ICommand), typeof(NavBar));

    public ICommand RightCommand
    {
        get => (ICommand)GetValue(RightCommandProperty);
        set => SetValue(RightCommandProperty, value);
    }

    // Show/hide left button
    public static readonly BindableProperty ShowLeftButtonProperty =
        BindableProperty.Create(
            nameof(ShowLeftButton),
            typeof(bool),
            typeof(NavBar),
            true,
            propertyChanged: OnShowButtonChanged);

    public bool ShowLeftButton
    {
        get => (bool)GetValue(ShowLeftButtonProperty);
        set => SetValue(ShowLeftButtonProperty, value);
    }

    // Show/hide right button
    public static readonly BindableProperty ShowRightButtonProperty =
        BindableProperty.Create(
            nameof(ShowRightButton),
            typeof(bool),
            typeof(NavBar),
            true,
            propertyChanged: OnShowButtonChanged);

    public bool ShowRightButton
    {
        get => (bool)GetValue(ShowRightButtonProperty);
        set => SetValue(ShowRightButtonProperty, value);
    }

    // Left command parameter
    public static readonly BindableProperty LeftCommandParameterProperty =
        BindableProperty.Create(nameof(LeftCommandParameter), typeof(object), typeof(NavBar));

    public object LeftCommandParameter
    {
        get => GetValue(LeftCommandParameterProperty);
        set => SetValue(LeftCommandParameterProperty, value);
    }

    // Right command parameter
    public static readonly BindableProperty RightCommandParameterProperty =
        BindableProperty.Create(nameof(RightCommandParameter), typeof(object), typeof(NavBar));

    public object RightCommandParameter
    {
        get => GetValue(RightCommandParameterProperty);
        set => SetValue(RightCommandParameterProperty, value);
    }

    // Title alignment
    public static readonly BindableProperty TitleAlignmentProperty =
        BindableProperty.Create(
            nameof(TitleAlignment),
            typeof(LayoutOptions),
            typeof(NavBar),
            LayoutOptions.Center);

    public LayoutOptions TitleAlignment
    {
        get => (LayoutOptions)GetValue(TitleAlignmentProperty);
        set => SetValue(TitleAlignmentProperty, value);
    }

    // Shared handler for left/right visibility changes
    private static void OnShowButtonChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is NavBar navBar)
            navBar.UpdateTitleAlignment();
    }

    private void UpdateTitleAlignment()
    {
        if (ShowLeftButton && !ShowRightButton)
            TitleAlignment = LayoutOptions.End;
        else if (!ShowLeftButton && ShowRightButton)
            TitleAlignment = LayoutOptions.Start;
        else
            TitleAlignment = LayoutOptions.Center;
    }
}
