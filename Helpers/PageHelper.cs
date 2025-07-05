using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiritualGiftsTest.Helpers;

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
}

