using SpiritualGiftsSurvey.Enums;
using SpiritualGiftsSurvey.Models;

namespace SpiritualGiftsSurvey.Views.Controls;

public class GiftScoreViewModel
{
    public GiftScoreViewModel(UserGiftScore model, string localizedGiftName)
    {
        Model = model;

        GiftName = localizedGiftName;

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

    public UserGiftScore Model { get; }
    public string GiftName { get; }
    public int Score => Model.Score;
    public double Progress { get; }
    public bool ShowMedal { get; }
    public Color MedalColor { get; }
}
