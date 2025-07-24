using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    public ResultsViewModel(
        IAggregatedServices aggregatedServices,
        IPreferences preferences)
        : base(aggregatedServices, preferences)
    {

    }

    [ObservableProperty] private SurveyResult? userGiftResult;
    [ObservableProperty] private string continueButtonText = string.Empty;

    public ObservableCollection<GiftScoreViewModel> AllGiftScores { get; } = new();

    partial void OnUserGiftResultChanged(SurveyResult? value)
    {
        if (value == null)
            return;

        IsLoading = true;

        value.RankGifts();

        // Populate the view models
        AllGiftScores.Clear();

        foreach (var score in value.Scores.OrderByDescending(x => x.Score))
        {
            var localizedGiftName = TranslationService.GetString(score.Gift.ToString(), score.Gift.ToString());
            AllGiftScores.Add(new GiftScoreViewModel(score, localizedGiftName, ViewGiftDescriptionCommand));
        }

        IsLoading = false;
    }

    public override void InitAsync()
    {
        IsLoading = true;

        if (string.IsNullOrEmpty(ContinueButtonText))
        {
            ContinueButtonText = TranslationService.GetString("Continue", "Continue");
        }

        if (string.IsNullOrEmpty(PageTopic))
        {
            PageTopic = TranslationService.GetString("SurveyResultsTitle", "Survey Results");
        }

        IsLoading = false;
    }

    public override void RefreshViewModel()
    {
        return;
    }

    [RelayCommand]
    private async Task ViewGiftDescriptionAsync(GiftScoreViewModel vm)
    {
        if (vm?.Model == null) 
            return;

        IsLoading = true;

        await NavigationService.NavigateAsync(Routes.GiftDescriptionPage, new Dictionary<string, object>
        {
            ["Gift"] = vm.Model,
            ["UserGiftResult"] = UserGiftResult
        });

        IsLoading = false;
    }

    [RelayCommand]
    private async Task ContinueAsync()
    {
        if (UserGiftResult == null) 
            return;

        IsLoading = true;

        await NavigationService.NavigateAsync(Routes.SendPage, new Dictionary<string, object>
        {
            ["UserGiftResult"] = UserGiftResult
        });

        IsLoading = false;
    }
}
