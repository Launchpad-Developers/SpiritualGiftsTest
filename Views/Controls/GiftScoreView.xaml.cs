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

        Console.WriteLine($"[GiftScoreView] OnBindingContextChanged called");
        
        // Clear and re-add gesture recognizer for recycled cells
        GestureRecognizers.Clear();

        if (BindingContext is GiftScoreViewModel vm)
        {
            Console.WriteLine($"[GiftScoreView] ✅ BindingContext is GiftScoreViewModel:");
            Console.WriteLine($"[GiftScoreView]    GiftName: {vm.GiftName}");
            Console.WriteLine($"[GiftScoreView]    Score: {vm.Score}");
            Console.WriteLine($"[GiftScoreView]    Progress: {vm.Progress:F2}");
            Console.WriteLine($"[GiftScoreView]    ShowMedal: {vm.ShowMedal}");
            Console.WriteLine($"[GiftScoreView]    MedalColor: {vm.MedalColor}");
            
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (sender, e) =>
            {
                vm.ViewGiftDescriptionCommand?.Execute(vm);
            };

            GestureRecognizers.Add(tapGesture);

            // Reset progress bar immediately to prevent showing stale data
            ProgressFill.WidthRequest = 0;
            Console.WriteLine($"[GiftScoreView] Reset ProgressFill.WidthRequest to 0");
            
            // Update progress bar after a brief delay to ensure container is measured
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(10), UpdateProgressBar);
        }
        else
        {
            Console.WriteLine($"[GiftScoreView] ❌ BindingContext is NOT GiftScoreViewModel! Type: {BindingContext?.GetType().Name ?? "null"}");
        }
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
        UpdateProgressBar();
    }

    private void UpdateProgressBar()
    {
        if (BindingContext is not GiftScoreViewModel vm)
        {
            Console.WriteLine($"[GiftScoreView.UpdateProgressBar] ❌ No valid BindingContext");
            ProgressFill.WidthRequest = 0;
            return;
        }

        double clampedProgress = Math.Clamp(vm.Progress, 0, 1);
        double width = BarContainer.Width;

        Console.WriteLine($"[GiftScoreView.UpdateProgressBar] {vm.GiftName}: BarContainer.Width={width:F1}, Progress={clampedProgress:F2}");

        if (width > 0)
        {
            double newWidth = width * clampedProgress;
            ProgressFill.WidthRequest = newWidth;
            Console.WriteLine($"[GiftScoreView.UpdateProgressBar] Set ProgressFill.WidthRequest = {newWidth:F1}");
        }
        else
        {
            Console.WriteLine($"[GiftScoreView.UpdateProgressBar] ⚠️ BarContainer.Width is 0, skipping update");
        }
    }
}
