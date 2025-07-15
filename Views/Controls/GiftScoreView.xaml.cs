using System.Diagnostics;
using System.Windows.Input;

namespace SpiritualGiftsSurvey.Views.Controls;

public partial class GiftScoreView : ContentView
{
    public GiftScoreView()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        GestureRecognizers.Clear();

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (sender, e) =>
        {
            ((GiftScoreViewModel)BindingContext).ViewGiftDescriptionCommand?.Execute(BindingContext);
        };

        GestureRecognizers.Add(tapGesture);
    }


    public static readonly BindableProperty TapCommandProperty =
        BindableProperty.Create(
            nameof(TapCommand),
            typeof(ICommand),
            typeof(GiftScoreView),
            default(ICommand));

    public ICommand? TapCommand
    {
        get => (ICommand?)GetValue(TapCommandProperty);
        set
        {
            Debug.WriteLine($"TapCommand bound: {value?.GetType().Name ?? "null"}");
            SetValue(TapCommandProperty, value);
        }
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        if (BindingContext is not GiftScoreViewModel vm)
            return;

        double clampedProgress = Math.Clamp(vm.Progress, 0, 1);
        double width = BarContainer.Width;

        ProgressFill.WidthRequest = width * clampedProgress;
    }
}
