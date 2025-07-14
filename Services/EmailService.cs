using SpiritualGiftsSurvey.Enums;
using SpiritualGiftsSurvey.Models;
using SpiritualGiftsSurvey.Utilities;
using System.Text;
using System.Text.Json;

namespace SpiritualGiftsSurvey.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(SurveyResult result);
    List<string> GetStoredEmails();
    bool SaveEmail(string email);
    bool RemoveEmail(string email);
}

public class EmailService : IEmailService
{
    private ITranslationService _translationService;
    private IDatabaseService _databaseService;

    public EmailService(
        ITranslationService translationService,
        IDatabaseService databaseService)
    {
        _translationService = translationService;
        _databaseService = databaseService;
    }

    public async Task<bool> SendEmailAsync(SurveyResult result)
    {
        if (result == null || string.IsNullOrWhiteSpace(result.Email))
            return false;

        var recipients = new List<string> { result.Email };
        var bccRecipients = GetStoredEmails();

        var subject = _translationService.GetString("EmailSubject", "Your Spiritual Gifts Survey Results");
        var body = GenerateHtmlEmail(result);

        try
        {
            var message = new EmailMessage
            {
                Subject = subject,
                Body = body,
                BodyFormat = EmailBodyFormat.Html,
                To = recipients,
                Bcc = bccRecipients
            };

            await Email.Default.ComposeAsync(message);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private string GenerateHtmlEmail(SurveyResult result)
    {
        var sb = new StringBuilder();

        sb.Append("<html><body>");
        sb.Append($"<h2>{_translationService.GetString("EmailSubject", "Spiritual Gifts Survey Results")}</h2>");
        sb.AppendFormat("<p><strong>{0}:</strong> {1} {2}</p>", _translationService.GetString("Name", "Name"), result.FirstName, result.LastName);
        sb.AppendFormat("<p><strong>{0}:</strong> {1}</p>", _translationService.GetString("Email", "Email"), result.Email);
        sb.Append($"<h3>{_translationService.GetString("Scores", "Scores")}:</h3><ul>");

        // Ensure gifts are ranked before sending
        result.RankGifts();
        var ranked = result.Scores
            .Where(s => s.GiftRank == GiftRank.Primary || s.GiftRank == GiftRank.Secondary)
            .OrderByDescending(s => s.Score);

        foreach (var score in ranked)
        {
            var desc = _databaseService.GetGiftDescription(score.GiftDescriptionGuid);
            var verses = _databaseService.GetVerses(score.GiftDescriptionGuid);
            sb.AppendFormat("<li><strong>{0}</strong>: {1}</li>", score.Gift, score.Score);

            if (desc != null)
            {
                sb.Append("<ul>");
                sb.AppendFormat("<li style='margin-left:20px;'>{0}</li>", desc.Description);

                if (verses != null && verses.Any())
                {
                    sb.Append("<li style='margin-left:20px;'><em>" + string.Join(", ", verses.Select(v => v.Reference)) + "</em></li>");
                }

                sb.Append("</ul>");
            }
        }

        sb.Append("</ul>");
        sb.Append("</body></html>");

        return sb.ToString();
    }

    public List<string> GetStoredEmails()
    {
        var stored = Preferences.Get(AppConstants.ReportingEmailsKey, null);
        return !string.IsNullOrEmpty(stored)
            ? JsonSerializer.Deserialize<List<string>>(stored) ?? new()
            : new();
    }

    public bool SaveEmail(string email)
    {
        var list = GetStoredEmails();
        if (!list.Contains(email, StringComparer.OrdinalIgnoreCase))
        {
            list.Add(email);
            Preferences.Set(AppConstants.ReportingEmailsKey, JsonSerializer.Serialize(list));

            return true;
        }

        return false;
    }

    public bool RemoveEmail(string email)
    {
        var list = GetStoredEmails();
        if (list.RemoveAll(e => e.Equals(email, StringComparison.OrdinalIgnoreCase)) > 0)
        {
            Preferences.Set(AppConstants.ReportingEmailsKey, JsonSerializer.Serialize(list));

            return true;
        }

        return false;
    }
}