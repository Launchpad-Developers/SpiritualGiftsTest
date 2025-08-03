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
public partial class ResultsViewModel(
    IAggregatedServices aggregatedServices,
    IPreferences preferences)
    : BaseViewModel(aggregatedServices, preferences)
{
    [ObservableProperty] private SurveyResult? userGiftResult;
    [ObservableProperty] private string continueButtonText = string.Empty;

    public ObservableCollection<GiftScoreViewModel> AllGiftScores { get; } = new();

    partial void OnUserGiftResultChanged(SurveyResult? value)
    {
        if (value?.Scores == null)
            return;

        if (!value.IsRanked)
        {
            _ = value.RankGiftsAsync();
        }

        _ = LoadUserGiftResultAsync(value);
    }

    public override async Task InitAsync()
    {
        IsLoading = true;
        await Task.Yield();

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
        await Task.Yield();

        await NavigationService.NavigateAsync(Routes.GiftDescriptionPage, new Dictionary<string, object>
        {
            ["Gift"] = vm.Model,
            ["UserGiftResult"] = UserGiftResult!
        });

        IsLoading = false;
    }

    [RelayCommand]
    private async Task ContinueAsync()
    {
        if (UserGiftResult == null)
            return;

        IsLoading = true;
        await Task.Yield();

        await NavigationService.NavigateAsync(Routes.SendPage, new Dictionary<string, object>
        {
            ["UserGiftResult"] = UserGiftResult!
        });

        IsLoading = false;
    }

    private async Task LoadUserGiftResultAsync(SurveyResult value)
    {
        IsLoading = true;
        await Task.Yield();

        try
        {
            AllGiftScores.Clear();

            foreach (var score in value.Scores.OrderByDescending(x => x.Score))
            {
                var localizedGiftName = TranslationService.GetString(score.Gift.ToString(), score.Gift.ToString());

                AllGiftScores.Add(new GiftScoreViewModel(score, localizedGiftName, ViewGiftDescriptionCommand));

                //Yield every 5 items to keep UI responsive
                //Prevents janky behavior on slower devices
                if (AllGiftScores.Count % 5 == 0)
                    await Task.Yield();
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}
