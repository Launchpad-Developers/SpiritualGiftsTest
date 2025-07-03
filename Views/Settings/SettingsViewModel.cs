using SpiritualGiftsTest.Interfaces;
using SpiritualGiftsTest.Models;
using SpiritualGiftsTest.Views.Shared;

namespace SpiritualGiftsTest.Views.Settings;

public class SettingsViewModel : BaseViewModel
{
    public SettingsViewModel(IAggregatedServices aggregatedServices) : base(aggregatedServices)
    {
        InitializeData();
    }

    private string _header;
    public string Header
    {
        get => _header;
        set { _header = value; OnPropertyChanged(nameof(Header)); }
    }

    public List<TranslationOptionModel> PrimaryLanguageOptions { get; set; }
    private TranslationOptionModel _selectedPrimaryLanguage;
    public TranslationOptionModel SelectedPrimaryLanguage
    {
        get => _selectedPrimaryLanguage;
        set
        {
            if (value == null || _selectedPrimaryLanguage == value) return;
            _selectedPrimaryLanguage = value;
            LanguageChanged();
        }
    }

    public List<TranslationOptionModel> ParallelLanguageOptions { get; set; }
    private TranslationOptionModel _selectedParallelLanguage;
    public TranslationOptionModel SelectedParallelLanguage
    {
        get => _selectedParallelLanguage;
        set
        {
            if (value == null || _selectedParallelLanguage == value) return;
            _selectedParallelLanguage = value;
            ParallelLanguageChanged();
        }
    }

    private string _tabletRequired;
    public string TabletRequired
    {
        get { return _tabletRequired; }
        set { _tabletRequired = value; OnPropertyChanged(nameof(TabletRequired)); }
    }

    private string _parallelMode;
    public string ParallelMode
    {
        get { return _parallelMode; }
        set { _parallelMode = value; OnPropertyChanged(nameof(ParallelMode)); }
    }

    private string _primaryLanguageTitle;
    public string PrimaryLanguageTitle
    {
        get { return _primaryLanguageTitle; }
        set { _primaryLanguageTitle = value; OnPropertyChanged(nameof(PrimaryLanguageTitle)); }
    }

    private string _parallelLanguageTitle;
    public string ParallelLanguageTitle
    {
        get { return _parallelLanguageTitle; }
        set { _parallelLanguageTitle = value; OnPropertyChanged(nameof(ParallelLanguageTitle)); }
    }

    private async void LanguageChanged()
    {
        await TranslationService.SetPrimaryLanguageForCode(SelectedPrimaryLanguage);
        MessagingCenter.Send(this, "LanguageChanged");
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

        PrimaryLanguageOptions = new List<TranslationOptionModel>();
        ParallelLanguageOptions = new List<TranslationOptionModel>();

        var languageOptions = TranslationService.PrimaryTranslationOptions;
        foreach (var option in languageOptions)
        {
            if (option.CodeOption == TranslationService.PrimaryLanguageCode)
            {
                option.Selected = true;
                _selectedPrimaryLanguage = option;
            }
            
            PrimaryLanguageOptions.Add(option);
        }

        var parallelLanguageOptions = TranslationService.ParallelTranslationOptions;
        foreach (var option in parallelLanguageOptions)
        {
            if (option.CodeOption == TranslationService.ParallelLanguageCode)
            {
                option.Selected = true;
                _selectedParallelLanguage = option;
            }

            ParallelLanguageOptions.Add(option);
        }

        OnPropertyChanged(nameof(PrimaryLanguageOptions));
        OnPropertyChanged(nameof(ParallelLanguageOptions));
        OnPropertyChanged(nameof(SelectedPrimaryLanguage));
        OnPropertyChanged(nameof(SelectedParallelLanguage));

        ParallelMode = lang.ParallelMode;
        TabletRequired = lang.OnlyAvailableOnTablets;

        PrimaryLanguageTitle = $"{lang.StudentLanguage} 1";
        ParallelLanguageTitle = $"{lang.TeacherLanguage} 2";

        IsLoading = false;
    }
}