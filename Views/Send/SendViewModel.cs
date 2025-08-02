using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Routing;
using SpiritualGiftsSurvey.Services;
using SpiritualGiftsSurvey.Views.Shared;
using System.Collections.ObjectModel;

namespace SpiritualGiftsSurvey.Views.Send;

[QueryProperty(nameof(UserGiftResult), "UserGiftResult")]
public partial class SendViewModel : BaseViewModel
{
    public SendViewModel(
        IAggregatedServices aggregatedServices,
        IPreferences preferences) : base(aggregatedServices, preferences)
    {
    }

    public ObservableCollection<Reflection> Reflections { get; } = new();

    private string _requiredTitle = string.Empty;
    private string _pleaseEnterName = string.Empty;
    private string _ok = string.Empty;

    [ObservableProperty] private SurveyResult? userGiftResult;
    [ObservableProperty] private string firstName = string.Empty;
    [ObservableProperty] private string lastName = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string firstNamePlaceholder = string.Empty;
    [ObservableProperty] private string lastNamePlaceholder = string.Empty;
    [ObservableProperty] private string emailPlaceholder = string.Empty;
    [ObservableProperty] private string continueButtonText = string.Empty;
    [ObservableProperty] private string reflectionsTitle = string.Empty;

    public async override Task InitAsync()
    {
        IsLoading = true;
        await Task.Yield();

        FirstNamePlaceholder = TranslationService.GetString("FirstName", "First Name");
        LastNamePlaceholder = TranslationService.GetString("LastName", "Last Name");
        EmailPlaceholder = TranslationService.GetString("Email", "Email");
        ContinueButtonText = TranslationService.GetString("Continue", "Continue");

        _requiredTitle = TranslationService.GetString("RequiredTitle", "Required");
        _pleaseEnterName = TranslationService.GetString("PleaseEnterName", "Please enter your name.");
        _ok = TranslationService.GetString("OK", "OK");

        PageTopic = TranslationService.GetString("SendTitle", "Send Your Results");
        ReflectionsTitle = TranslationService.GetString("ReflectionsTitle", "Reflections");

        Reflections.Clear();
        var reflections = DatabaseService.GetReflections(TranslationService.CurrentLanguageCode);
        foreach (var reflection in reflections)
        {
            Reflections.Add(reflection);
        }

        IsLoading = false;
    }

    public override void RefreshViewModel()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
    }

    [RelayCommand]
    private async Task ContinueAsync()
    {
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            await NotifyUserAsync(_requiredTitle, _pleaseEnterName, _ok);
            return;
        }

        if (UserGiftResult is null)
            return;

        IsLoading = true;

        UserGiftResult.FirstName = FirstName.Trim();
        UserGiftResult.LastName = LastName.Trim();
        UserGiftResult.Email = Email.Trim();

        // Send Email
        await EmailService.SendEmailAsync(UserGiftResult);

        // Save to local database
        await DatabaseService.SaveUserGiftResultAsync(UserGiftResult);

        await NavigationService.GoBackAsync(Routes.WelcomePage);

        IsLoading = false;
    }

    partial void OnUserGiftResultChanged(SurveyResult? value)
    {
        if (value == null || value.Scores == null)
            return;
    }
}
