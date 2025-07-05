using Microsoft.Maui.Controls;
using SpiritualGiftsTest.Enums;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace SpiritualGiftsTest.ViewModels;

/// <summary>
/// Represents a single question with its metadata and user response.
/// </summary>
public class QuestionViewModel : INotifyPropertyChanged
{
    private string _questionText = string.Empty;
    private Guid _questionId;
    private UserValue _userValue = UserValue.NotAtAll;
    private bool _answered;

    public QuestionViewModel()
    {
        SetValueCommand = new Command<UserValue>(val =>
        {
            UserValue = val;
            Answered = true;
        });
    }

    /// <summary>
    /// Command to set the UserValue and mark answered.
    /// </summary>
    public ICommand SetValueCommand { get; }

    /// <summary>
    /// The full text of the question.
    /// </summary>
    public string QuestionText
    {
        get => _questionText;
        set
        {
            if (_questionText != value)
            {
                _questionText = value;
                OnPropertyChanged(nameof(QuestionText));
            }
        }
    }

    /// <summary>
    /// A unique identifier for the question.
    /// </summary>
    public Guid QuestionId
    {
        get => _questionId;
        set
        {
            if (_questionId != value)
            {
                _questionId = value;
                OnPropertyChanged(nameof(QuestionId));
            }
        }
    }

    /// <summary>
    /// The user's selected value for this question.
    /// </summary>
    public UserValue UserValue
    {
        get => _userValue;
        set
        {
            if (_userValue != value)
            {
                _userValue = value;
                OnPropertyChanged(nameof(UserValue));
            }
        }
    }

    /// <summary>
    /// Indicates whether the question has been answered.
    /// </summary>
    public bool Answered
    {
        get => _answered;
        set
        {
            if (_answered != value)
            {
                _answered = value;
                OnPropertyChanged(nameof(Answered));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
