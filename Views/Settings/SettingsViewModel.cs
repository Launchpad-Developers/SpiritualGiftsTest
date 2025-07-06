using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SpiritualGiftsTest.Helpers;
using SpiritualGiftsTest.Messages;
using SpiritualGiftsTest.Models;
using SpiritualGiftsTest.Services;
using SpiritualGiftsTest.Views.Shared;
using System.Runtime.Versioning;

namespace SpiritualGiftsTest.Views.Settings;

[SupportedOSPlatform("android")]
[SupportedOSPlatform("ios")]
public partial class SettingsViewModel : BaseViewModel
{
    public SettingsViewModel(
        IAggregatedServices aggregatedServices,
        IPreferences preferences) 
        : base(aggregatedServices, preferences)
    {
        InitializeData();
    }

    [ObservableProperty]
    private string header = string.Empty;

    [ObservableProperty]
    private string languageTitle = string.Empty;

    public List<LanguageOption> LanguageOptions { get; set; }
        = new List<LanguageOption>();

    [ObservableProperty]
    private Translation currentTranslation = new();

    partial void OnCurrentTranslationChanged(Translation value)
    {
        if (value == null) return;
        CurrentTranslationChanged();
    }

    private async void CurrentTranslationChanged()
    {
        IsLoading = true;

        try
        {
            await TranslationService.SetLanguageByCodeAsync(CurrentTranslation.Code);
        }
        finally
        {
            IsLoading = false;
            await NavBack();
        }
    }

    public void InitializeData()
    {
        IsLoading = true;

        try
        {
            CurrentTranslation = TranslationService.Language;
            FlowDirection = CurrentTranslation.FlowDirection.Equals("RTL") ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            LoadingText = CurrentTranslation.AppStrings.Get("Loading", "Loading");
            PageTopic = CurrentTranslation.AppStrings.Get("Settings", "Settings");
            LanguageOptions = CurrentTranslation.LanguageOptions;
            LanguageTitle = TranslationService.CurrentLanguageDisplayName;
        }
        finally
        {
            IsLoading = false;
        }
    }
}