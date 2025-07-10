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

    public QuestionViewModel()
    {
    }

    [RelayCommand]
    private void SetValue(UserValue value)
    {
        UserValue = value;
        Answered = true;
    }

    partial void OnQuestionTextChanged(string value)
    {
        CalculateCellHeight(value);
    }

    public void CalculateCellHeight(string questionText)
    {
        double fontSize = 18;
        double lineHeight = fontSize * 1.2;
        int charsPerLine = 35;
        int textLength = questionText?.Length ?? 0;

        double estimatedLines = Math.Ceiling((double)textLength / charsPerLine);

        double padding = ShowButtons ? 320 : 100;

        //CellHeight = Math.Max(estimatedLines * lineHeight + padding, 60);
        CellHeight = (estimatedLines * lineHeight) + padding;
    }

    partial void OnAnsweredChanged(bool value)
    {
        BorderColor = value ? (Color?)Application.Current?.Resources["Green"] ?? Colors.Green 
            : (Color?)Application.Current?.Resources["Black"] ?? Colors.Black;
    }

    public void MarkQuestionUnanswered()
    {
        BorderColor = (Color?)Application.Current?.Resources["Red"] ?? Colors.Red;
    }
}
