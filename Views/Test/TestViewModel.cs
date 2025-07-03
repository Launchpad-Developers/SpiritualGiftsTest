using System.Collections.Generic;
using System.Windows.Input;
using SpiritualGiftsTest.Interfaces;
using SpiritualGiftsTest.Models;
using SpiritualGiftsTest.Views.Shared;

namespace SpiritualGiftsTest.Views.Test;

public class TestNavParameter
{
    public int TargetPage { get; set; }
    public string TargetPageTopic { get; set; }
}

public class TestViewModel : BaseViewModel
{
    protected string NextPage { get; set; }
    public int PageNumber { get; set; }

    public Dictionary<int, ContentView> ContentViews { get; set; }

    public TestViewModel(IAggregatedServices aggregatedServices) : base(aggregatedServices)
    {
        LeaveStudyCommand = new Command(OnLeaveStudyCommand);
        NavigatedCommand = new Command<TestNavParameter>(OnNavigatedCommand);

        InitializeData();
    }

    public ICommand LeaveStudyCommand { get; }
    public ICommand NavigatedCommand { get; }

    private async void OnLeaveStudyCommand()
    {
        var result = await Application.Current.MainPage.DisplayAlert(PrimaryLanguageTranslation.Quit, PrimaryLanguageTranslation.AreYouSure, PrimaryLanguageTranslation.Yes, PrimaryLanguageTranslation.No);
        if (result)
            await NavBack();
    }

    private void OnNavigatedCommand(TestNavParameter parameter)
    {
        PageOf = $"{PrimaryLanguageTranslation.Page} {parameter.TargetPage} {PrimaryLanguageTranslation.Of} 20";
        PageTopic = parameter.TargetPageTopic;
    }

    public virtual void InitializeData()
    {
        PrimaryLanguageTranslation = TranslationService.PrimaryLanguage;

        FlowDirection = PrimaryLanguageTranslation.LanguageTextDirection.Equals("RL") ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        SpiritualGiftsTest = PrimaryLanguageTranslation.SpiritualGiftsTestTitle;
        PageOf = $"{PrimaryLanguageTranslation.Page} 1 {PrimaryLanguageTranslation.Of} 20";
        LoadingText = PrimaryLanguageTranslation.Loading;
    }

    private TranslationModel _primaryLanguageTranslation;
    public TranslationModel PrimaryLanguageTranslation
    {
        get { return _primaryLanguageTranslation; }
        set { _primaryLanguageTranslation = value; OnPropertyChanged(nameof(PrimaryLanguageTranslation)); }
    }

    private bool _showBackNav;
    public bool ShowBackNav
    {
        get { return _showBackNav; }
        set { _showBackNav = value; OnPropertyChanged(nameof(ShowBackNav)); }
    }

    private bool _showForwardNav;
    public bool ShowForwardNav
    {
        get { return _showForwardNav; }
        set { _showForwardNav = value; OnPropertyChanged(nameof(ShowForwardNav)); }
    }

    private string _SpiritualGiftsTest;
    public string SpiritualGiftsTest
    {
        get { return _SpiritualGiftsTest; }
        set { _SpiritualGiftsTest = value; OnPropertyChanged(nameof(SpiritualGiftsTest)); }
    }
}
