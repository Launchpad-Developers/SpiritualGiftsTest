using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
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
    private string tabletRequired = string.Empty;

    [ObservableProperty]
    private string parallelMode = string.Empty;

    [ObservableProperty]
    private string languageTitle = string.Empty;

    [ObservableProperty]
    private string parallelLanguageTitle = string.Empty;

    [ObservableProperty]
    private TranslationOptionModel selectedLanguage = new();

    [ObservableProperty]
    private TranslationOptionModel selectedParallelLanguage = new();

    public List<TranslationOptionModel> LanguageOptions { get; set; }
        = new List<TranslationOptionModel>();
    public List<TranslationOptionModel> ParallelLanguageOptions { get; set; }
        = new List<TranslationOptionModel>();

    partial void OnSelectedLanguageChanged(TranslationOptionModel value)
    {
        if (value == null) return;
        LanguageChanged();
    }

    partial void OnSelectedParallelLanguageChanged(TranslationOptionModel value)
    {
        if (value == null) return;
        ParallelLanguageChanged();
    }

    private async void LanguageChanged()
    {
        await TranslationService.SetPrimaryLanguageForCode(SelectedLanguage);

        WeakReferenceMessenger.Default.Send(new LanguageChangedMessage(SelectedLanguage));

        await NavBack();
    }

    private async void ParallelLanguageChanged()
    {
        await TranslationService.SetParallelLanguageForCode(SelectedParallelLanguage);
        OnPropertyChanged(nameof(SelectedParallelLanguage));
    }

    public void InitializeData()
    {
		IsLoading = true;

        var lang = TranslationService.PrimaryLanguage;

        FlowDirection = lang.LanguageTextDirection.Equals("RL") ? FlowDirection.RightToLeft :  FlowDirection.LeftToRight;

        LoadingText = lang.Loading;
		PageTopic = lang.Settings;

        LanguageOptions = new List<TranslationOptionModel>();
        ParallelLanguageOptions = new List<TranslationOptionModel>();

        var languageOptions = TranslationService.PrimaryTranslationOptions;
        foreach (var option in languageOptions)
        {
            if (option.CodeOption == TranslationService.PrimaryLanguageCode)
            {
                option.Selected = true;
                SelectedLanguage = option;
            }
            
            LanguageOptions.Add(option);
        }

        var parallelLanguageOptions = TranslationService.ParallelTranslationOptions;
        foreach (var option in parallelLanguageOptions)
        {
            if (option.CodeOption == TranslationService.ParallelLanguageCode)
            {
                option.Selected = true;
                SelectedParallelLanguage = option;
            }

            ParallelLanguageOptions.Add(option);
        }

        OnPropertyChanged(nameof(LanguageOptions));
        OnPropertyChanged(nameof(ParallelLanguageOptions));
        OnPropertyChanged(nameof(SelectedLanguage));
        OnPropertyChanged(nameof(SelectedParallelLanguage));

        ParallelMode = lang.ParallelMode;
        TabletRequired = lang.OnlyAvailableOnTablets;

        LanguageTitle = $"{lang.StudentLanguage} 1";
        ParallelLanguageTitle = $"{lang.TeacherLanguage} 2";

        IsLoading = false;
    }
}