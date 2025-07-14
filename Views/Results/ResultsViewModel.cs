using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpiritualGiftsSurvey.Enums;
using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Routing;
using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Views.Controls;
using SpiritualGiftsSurvey.Views.Shared;
using System.Collections.ObjectModel;

namespace SpiritualGiftsSurvey.Views.Results;

[QueryProperty(nameof(UserGiftResult), "UserGiftResult")]
public partial class ResultsViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;

    public ResultsViewModel(
        IAggregatedServices aggregatedServices,
        IPreferences preferences,
        INavigationService navigationService)
        : base(aggregatedServices, preferences)
    {
        _navigationService = navigationService;
    }

    [ObservableProperty] private SurveyResult? userGiftResult;

    [ObservableProperty] private string continueButtonText = string.Empty;

    public ObservableCollection<GiftScoreViewModel> AllGiftScores { get; } = new();

    partial void OnUserGiftResultChanged(SurveyResult? value)
    {
        if (value == null)
            return;

        value.RankGifts();

        // Populate the view models
        AllGiftScores.Clear();

        foreach (var score in value.Scores.OrderByDescending(x => x.Score))
        {
            var localizedGiftName = TranslationService.GetString(score.Gift.ToString(), score.Gift.ToString());
            AllGiftScores.Add(new GiftScoreViewModel(score, localizedGiftName));
        }
    }

    public override void InitAsync()
    {
        ContinueButtonText = TranslationService.GetString("Continue", "Continue");
        PageTopic = TranslationService.GetString("SurveyResultsTitle", "Survey Results");
    }

    [RelayCommand]
    private async Task ViewGiftDescriptionAsync(GiftScoreViewModel vm)
    {
        if (vm?.Model == null) return;

        await _navigationService.NavigateAsync("GiftDescriptionPage", new Dictionary<string, object>
        {
            ["Gift"] = vm.Model
        });
    }

    [RelayCommand]
    private async Task ContinueAsync()
    {
        await _navigationService.GoBackAsync(Routes.WelcomePage);
    }
}
