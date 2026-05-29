using CommunityToolkit.Mvvm.ComponentModel;
using SpiritualGiftsSurvey.Enums;
using SpiritualGiftsSurvey.Models;
using System.Windows.Input;

namespace SpiritualGiftsSurvey.Views.Controls;

public partial class GiftScoreViewModel : ObservableObject
{
    [ObservableProperty] private UserGiftScore model;
    [ObservableProperty] private string giftName;
    [ObservableProperty] private int score;
    [ObservableProperty] private double progress;
    [ObservableProperty] private bool showMedal;
    [ObservableProperty] private Color medalColor;
    [ObservableProperty] private ICommand viewGiftDescriptionCommand;

    public GiftScoreViewModel(UserGiftScore model, string localizedGiftName, ICommand command)
    {
        Model = model;
        GiftName = localizedGiftName;
        Score = model.Score;
        ViewGiftDescriptionCommand = command;

        Progress = model.MaxScore > 0
            ? (double)model.Score / model.MaxScore
            : 0;

        MedalColor = model.GiftRank switch
        {
            GiftRank.Primary => Colors.Gold,
            GiftRank.Secondary => Colors.Silver,
            _ => Colors.Transparent
        }; 
        
        ShowMedal = model.GiftRank != GiftRank.None;
    }
}
