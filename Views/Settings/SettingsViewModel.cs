using CommunityToolkit.Mvvm.ComponentModel;
using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Views.Shared;
using System.Runtime.Versioning;

namespace SpiritualGiftsSurvey.Views.Settings;

[SupportedOSPlatform("android")]
[SupportedOSPlatform("ios")]
public partial class SettingsViewModel : BaseViewModel
{
    public SettingsViewModel(
        IAggregatedServices aggregatedServices,
        IPreferences preferences) 
        : base(aggregatedServices, preferences)
    {

    }

    [ObservableProperty]
    private string header = string.Empty;

    [ObservableProperty]
    private List<LanguageOption> languageOptions = new();

    [ObservableProperty]
    private LanguageOption? selectedLanguage;

    [ObservableProperty]
    private string languageTitle = "English";

    partial void OnSelectedLanguageChanged(LanguageOption? value)
    {
        if (value == null)
            return;

        _ = TranslationService.SetLanguageByCodeAsync(value.CodeOption);

        // If your LanguageOptions might change display names, refresh them:
        LanguageOptions = TranslationService.GetLanguageOptions();
        LanguageTitle = TranslationService.CurrentLanguageDisplayName;
    }

    public override Task InitAsync(INavigation nav)
    {
        FlowDirection = TranslationService.FlowDirection;
        LoadingText = TranslationService.GetString("Loading", "Loading");
        PageTopic = TranslationService.GetString("Settings", "Settings");
        LanguageTitle = TranslationService.CurrentLanguageDisplayName;

        var currentCode = TranslationService.CurrentLanguageCode;
        LanguageOptions = TranslationService.GetLanguageOptions();
        SelectedLanguage = LanguageOptions.FirstOrDefault(lo => lo.CodeOption == currentCode);

        return Task.CompletedTask;
    }
}