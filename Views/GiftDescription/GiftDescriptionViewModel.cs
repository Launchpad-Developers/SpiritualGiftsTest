using CommunityToolkit.Mvvm.ComponentModel;
using SpiritualGiftsSurvey.Enums;
using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Views.Shared;

namespace SpiritualGiftsSurvey.Views.GiftDescription;

[QueryProperty(nameof(Gift), "Gift")]
public partial class GiftDescriptionViewModel : BaseViewModel
{
    public GiftDescriptionViewModel(
        IAggregatedServices aggregatedServices,
        IPreferences preferences)
        : base(aggregatedServices, preferences)
    {
    }

    [ObservableProperty]
    private UserGiftScore gift;

    [ObservableProperty]
    private string giftName = string.Empty;

    [ObservableProperty]
    private string giftDescriptionText = string.Empty;

    [ObservableProperty]
    private string giftScriptures = string.Empty;

    [ObservableProperty]
    private string scriptureLabel = string.Empty;

    [ObservableProperty]
    private string descriptionLabel = string.Empty;

    partial void OnGiftChanged(UserGiftScore value)
    {
        var gift = value.GiftName.ToGiftEnum();
        var verses = DatabaseService.GetVerses(value.GiftDescriptionGuid);
        var desc = DatabaseService.GetGiftDescription(TranslationService.CurrentLanguageCode, Gift.Gift);


        GiftName = desc?.Translation ?? value.Gift.ToString();
        GiftDescriptionText = desc?.Description ?? "No description found";
        GiftScriptures = string.Join("\n", verses?.Select(v => v.Reference) ?? []);
        PageTopic = string.Format(TranslationService.GetString("TheGiftOf", "The gift of {0}"), GiftName);
    }


    public override void InitAsync()
    {
        // Load UI label text via TranslationService
        ScriptureLabel = TranslationService.GetString("Scriptures", "Scriptures");
        DescriptionLabel = TranslationService.GetString("Description", "Description");
    }

}
