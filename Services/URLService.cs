using CommunityToolkit.Mvvm.ComponentModel;
using SpiritualGiftsTest.Models;
using SpiritualGiftsTest.Utilities;
using System.Net;

namespace SpiritualGiftsTest.Services;

public interface IURLService
{
    Task<string?> GetFullDatabaseJson();
    Task<string?> GetRemoteDatabaseInfoJson();
    Task<string?> GetAllLanguageJson();
    Task<string?> GetLanguageCodeJson();
}

public partial class URLService : ObservableObject, IURLService
{
    private HttpClient client { get; set; }

    public string AuthKey
    {
        get
        {
            return string.Empty;
        }
    }

    //TODO Move to constants or secrets file
    public string BaseURL { get { return "https://only-one-name-2-default-rtdb.firebaseio.com/"; } }

    [ObservableProperty]
    private List<TranslationOptionModel> _languageCodes = new();

    public URLService()
    {
        client = new HttpClient();
        client.MaxResponseContentBufferSize = 256000;
    }

    private async Task<Tuple<string, TitledException>> PostAuthorizedRequest(Uri uri)
    {
        var access = Connectivity.Current.NetworkAccess;

        if (access != NetworkAccess.Internet)
            return new Tuple<string, TitledException>(string.Empty, new TitledException("Error", "No Connection"));

        HttpResponseMessage response = await client.GetAsync(uri);

        if (response.IsSuccessStatusCode)
        {
            if (response.StatusCode <= 0)
            {
                return new Tuple<string, TitledException>(string.Empty, new TitledException("Error", "Connection Lost"));
            }
            else if (response.StatusCode != HttpStatusCode.InternalServerError && response.StatusCode != HttpStatusCode.OK)
            {
                var text = response.Content.ToString();
                if (!string.IsNullOrEmpty(text))
                {
                    return new Tuple<string, TitledException>(string.Empty, new TitledException("Response Failed", $"{response.StatusCode} - {text}"));
                }
                else
                {
                    return new Tuple<string, TitledException>(string.Empty, new TitledException("Error", "Connection Lost"));
                }
            }
            else if (response.StatusCode != HttpStatusCode.OK)
            {
                return new Tuple<string, TitledException>(string.Empty, new TitledException("Error", "Connection Lost"));
            }
            else //Winner winner chicken dinner!
            {
                var responseContent = await response.Content.ReadAsStringAsync(); // Fix: Ensure responseContent is awaited and properly assigned.

                if (string.IsNullOrEmpty(responseContent) || responseContent.Contains("Error")) // Fix: Use the awaited responseContent directly.
                {
                    return new Tuple<string, TitledException>(string.Empty, new TitledException("Error", $"Status Code: {response.StatusCode}\nContent:{responseContent}"));
                }
                else
                {
                    return new Tuple<string, TitledException>(responseContent, default!);
                }
            }
        }
        else
        {
            return new Tuple<string, TitledException>(string.Empty, new TitledException("Error", $"Status Code: {response.StatusCode}\nContent:{response.Content}"));
        }
    }

	public async Task<string?> GetFullDatabaseJson()
    {
        var uri = new Uri($"{ BaseURL }.json");
        Tuple<string, TitledException> raw = await PostAuthorizedRequest(uri);

		if (raw.Item2 != null)
			return null;
		else
			return raw.Item1;
    }

	public async Task<string?> GetRemoteDatabaseInfoJson()
	{
        var uri = new Uri($"{ BaseURL }DatabaseInfo/.json");
        Tuple<string, TitledException> raw = await PostAuthorizedRequest(uri);

        if (raw.Item2 != null)
            return null;
        else
            return raw.Item1;
	}

	public async Task<string?> GetAllLanguageJson()
	{
		var uri = new Uri($"{ BaseURL }Translations/.json");
        Tuple<string, TitledException> raw = await PostAuthorizedRequest(uri);

		if (raw.Item2 != null)
			return null;
		else
			return raw.Item1;
    }

	public async Task<string?> GetLanguageCodeJson()
	{
        var uri = new Uri($"{ BaseURL }TranslationOptions/.json");
        Tuple<string, TitledException> raw = await PostAuthorizedRequest(uri);

		if (raw.Item2 != null)
			return null;
		else
			return raw.Item1;
    }
}
