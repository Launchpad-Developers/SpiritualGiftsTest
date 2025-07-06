using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpiritualGiftsTest.Models;
using SpiritualGiftsTest.Services;
using SpiritualGiftsTest.Views.Shared;
using System.Runtime.Versioning;
using System.Windows.Input;

namespace SpiritualGiftsTest.Views.Test;

public class TestNavParameter
{
    public int TargetPage { get; set; }
    public string TargetPageTopic { get; set; } = string.Empty;
}

[SupportedOSPlatform("android")]
[SupportedOSPlatform("ios")]
public partial class TestViewModel : BaseViewModel
{

    public Dictionary<int, ContentView> ContentViews { get; set; } = new();

    public TestViewModel(
        IAggregatedServices aggregatedServices,
        IPreferences preferences) : base(aggregatedServices, preferences)
    {
        InitializeData();
    }

    public virtual void InitializeData()
    {
        PrimaryLanguageTranslation = TranslationService.PrimaryLanguage;

        FlowDirection = PrimaryLanguageTranslation.LanguageTextDirection.Equals("RL") ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        SpiritualGiftsTest = PrimaryLanguageTranslation.SpiritualGiftsTestTitle;
        PageOf = $"{PrimaryLanguageTranslation.Page} 1 {PrimaryLanguageTranslation.Of} 20";
        LoadingText = PrimaryLanguageTranslation.Loading;
    }

    [ObservableProperty]
    private string nextPage = string.Empty;

    [ObservableProperty]
    private int pageNumber;

    [ObservableProperty]
    private TranslationModel primaryLanguageTranslation = new();

    [ObservableProperty]
    private bool showBackNav;

    [ObservableProperty]
    private bool showForwardNav;

    [ObservableProperty]
    private string spiritualGiftsTest = string.Empty;

    [RelayCommand]
    private async Task OnLeaveSurveyAsync()
    {
        var result = await ConfirmUserAsync(
            PrimaryLanguageTranslation.Quit,
            PrimaryLanguageTranslation.AreYouSure,
            PrimaryLanguageTranslation.Yes,
            PrimaryLanguageTranslation.No);

        if (result)
            await NavBack();
    }

    [RelayCommand]
    private void Navigate(TestNavParameter parameter)
    {
        PageOf = $"{PrimaryLanguageTranslation.Page} {parameter.TargetPage} {PrimaryLanguageTranslation.Of} 20";
        PageTopic = parameter.TargetPageTopic;
    }
}
