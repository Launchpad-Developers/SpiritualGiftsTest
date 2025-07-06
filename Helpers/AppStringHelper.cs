using SpiritualGiftsTest.Models;

namespace SpiritualGiftsTest.Helpers;

public static class AppStringHelper
{
    /// <summary>
    /// Looks up an app string by key. 
    /// Falls back to a default value if not found.
    /// </summary>
    /// <param name="appStrings">The list of AppString records for this language.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="defaultValue">The fallback value if the key is missing.</param>
    /// <returns>The translated value or the fallback.</returns>
    public static string Get(this IEnumerable<AppString> appStrings, string key, string defaultValue = "")
    {
        if (string.IsNullOrWhiteSpace(key) || appStrings == null)
            return defaultValue;

        return appStrings.FirstOrDefault(x => x.Key == key)?.Value ?? defaultValue;
    }
}

