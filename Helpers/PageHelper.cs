using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpiritualGiftsSurvey.Helpers;

public static class PageHelper
{
    public static Page? GetCurrentPage() =>
        Application.Current?.Windows.FirstOrDefault()?.Page;

    public static async Task ShowAlert(string title, string message, string cancel)
    {
        var page = GetCurrentPage();
        if (page is not null)
        {
            await page.DisplayAlert(title, message, cancel);
        }
    }

    public static async Task<bool> ShowConfirm(string title, string message, string accept, string cancel)
    {
        var page = GetCurrentPage();
        if (page is not null)
        {
            return await page.DisplayAlert(title, message, accept, cancel);
        }

        // If no window: safe fallback
        return false;
    }

    public static string FormatFlatDate(string? flatDate, string fallback = "Unknown")
    {
        if (string.IsNullOrWhiteSpace(flatDate) || flatDate.Length != 10)
            return fallback;

        if (DateTime.TryParseExact(
            flatDate,
            "yyyy-MM-dd",
            null,
            System.Globalization.DateTimeStyles.None,
            out var parsedDate))
        {
            return parsedDate.ToString("MMMM dd, yyyy");
        }

        return fallback;
    }
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Simple but solid regex for typical email patterns
        var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
    }

}

