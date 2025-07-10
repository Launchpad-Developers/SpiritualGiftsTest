using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpiritualGiftsSurvey.Enums;
using System.ComponentModel;

namespace SpiritualGiftsSurvey.Models;

/// <summary>
/// Represents a single question with its metadata and user response.
/// </summary>
public partial class QuestionViewModel : ObservableObject, INotifyPropertyChanged
{
    private readonly Color _azureBlue;
    private readonly Color _emeraldGreen;
    private readonly Color _dangerRed;

    public QuestionViewModel()
    {
        _azureBlue = GetResourceColor("AzureBlue", Colors.Blue);
        _emeraldGreen = GetResourceColor("EmeraldGreen", Colors.Green);
        _dangerRed = GetResourceColor("DangerRed", Colors.Red);

        NotAtAllButtonColor = _azureBlue;
        LittleButtonColor = _azureBlue;
        SomeButtonColor = _azureBlue;
        MuchButtonColor = _azureBlue;
    }

    [ObservableProperty]
    private string notAtAll = string.Empty;

    [ObservableProperty]
    private string little = string.Empty;

    [ObservableProperty]
    private string some = string.Empty;

    [ObservableProperty]
    private string much = string.Empty;

    [ObservableProperty]
    private string questionText = string.Empty;

    [ObservableProperty]
    private Guid questionId;

    [ObservableProperty]
    private UserValue userValue = UserValue.DidNotAnswer;

    [ObservableProperty]
    private bool answered;

    [ObservableProperty]
    private double cellHeight = -1;

    [ObservableProperty]
    private Color borderColor = Color.FromArgb("#000000");

    [ObservableProperty]
    private bool showButtons;
    
    [ObservableProperty]
    private string questionOf = string.Empty;

    [ObservableProperty]
    private Color notAtAllButtonColor;

    [ObservableProperty]
    private Color littleButtonColor;

    [ObservableProperty]
    private Color someButtonColor;

    [ObservableProperty]
    private Color muchButtonColor;

    [ObservableProperty]
    private Thickness questionMargin = new Thickness(30, 10, 30, 10);

    [RelayCommand]
    private void SetValue(UserValue value)
    {
        UserValue = value;
        Answered = true;

        // Reset all to default
        NotAtAllButtonColor = _azureBlue;
        LittleButtonColor = _azureBlue;
        SomeButtonColor = _azureBlue;
        MuchButtonColor = _azureBlue;

        // Set selected to highlight
        switch (value)
        {
            case UserValue.NotAtAll:
                NotAtAllButtonColor = _emeraldGreen;
                break;
            case UserValue.Little:
                LittleButtonColor = _emeraldGreen;
                break;
            case UserValue.Some:
                SomeButtonColor = _emeraldGreen;
                break;
            case UserValue.Much:
                MuchButtonColor = _emeraldGreen;
                break;
        }
    }

    partial void OnAnsweredChanged(bool value)
    {
        BorderColor = value ? _emeraldGreen ?? Colors.Green 
            : (Color?)Application.Current?.Resources["Black"] ?? Colors.Black;
    }

    public void MarkQuestionUnanswered()
    {
        BorderColor = _dangerRed ?? Colors.Red;
    }

    private static Color GetResourceColor(string key, Color fallback)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color)
        {
            return color;
        }
        return fallback;
    }
}
