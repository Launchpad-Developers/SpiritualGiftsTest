using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using SpiritualGiftsTest.Enums;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace SpiritualGiftsTest.Models;

/// <summary>
/// Represents a single question with its metadata and user response.
/// </summary>
public partial class QuestionViewModel : ObservableObject, INotifyPropertyChanged
{
    [ObservableProperty]
    private string questionText = string.Empty;

    [ObservableProperty]
    private Guid questionId;

    [ObservableProperty]
    private UserValue userValue = UserValue.NotAtAll;

    [ObservableProperty]
    private bool answered;

    public QuestionViewModel()
    {
    }

    [RelayCommand]
    private void SetValue(UserValue value)
    {
        UserValue = value;
        Answered = true;
    }
}
