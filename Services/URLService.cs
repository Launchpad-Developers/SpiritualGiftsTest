using SpiritualGiftsTest.Interfaces;
using SpiritualGiftsTest.Models;
using SpiritualGiftsTest.Utilities;
using Plugin.Connectivity;
using System.Net;

namespace SpiritualGiftsTest.Services;

public class URLService : IURLService
{
    private HttpClient client { get; set; }

    public string AuthKey
    {
        get
        {
            return string.Empty;
        }
    }

    public string BaseURL { get { return "https://only-one-name-2-default-rtdb.firebaseio.com/"; } }

    private List<TranslationOptionModel> _languageCodes;
	public List<TranslationOptionModel> LanguageCodes
    {
        get { return _languageCodes; }
        private set { _languageCodes = value; }
    }

    public URLService()
    {
        client = new HttpClient();
        client.MaxResponseContentBufferSize = 256000;
    }

    private async Task<Tuple<string, TitledException>> PostAuthorizedRequest(Uri uri)
    {
		if (!CrossConnectivity.Current.IsConnected)
            return new Tuple<string, TitledException>(null, new TitledException("Error", "No Connection")); 
        
        HttpResponseMessage response = await client.GetAsync(uri);

        if (response.IsSuccessStatusCode)
        {
            if (response.StatusCode <= 0)
            {
                return new Tuple<string, TitledException>(null, new TitledException("Error", "Connection Lost"));                 
            }
            else if (response.StatusCode != HttpStatusCode.InternalServerError && response.StatusCode != HttpStatusCode.OK)
            {
                var text = response.Content.ToString();
                if (!string.IsNullOrEmpty(text))
                {
                    return new Tuple<string, TitledException>(null, new TitledException("Response Failed", $"{ response.StatusCode } - { text }"));
                }
                else
                {
                    return new Tuple<string, TitledException>(null, new TitledException("Error", "Connection Lost"));
                }
            }
            else if (response.StatusCode != HttpStatusCode.OK)
            {
                return new Tuple<string, TitledException>(null, new TitledException("Error", "Connection Lost"));
            }
            else //Winner winner chicken dinner!
            {
                var responseContent = response.Content.ReadAsStringAsync();

                if (String.IsNullOrEmpty(responseContent.ToString()) || responseContent.ToString().Contains("Error"))
                {
                    return new Tuple<string, TitledException>(null, new TitledException("Error", $"Status Code: { response.StatusCode }\nContent:{ responseContent }"));
                }
                else
                {
                    return new Tuple<string, TitledException>(responseContent.Result, null);
                }
            }
        }
        else
        {
            return new Tuple<string, TitledException>(null, new TitledException("Error", $"Status Code: { response.StatusCode }\nContent:{ response.Content }"));
        }
    }

	public async Task<string> GetFullDatabaseJson()
    {
        var uri = new Uri($"{ BaseURL }.json");
        Tuple<string, TitledException> raw = await PostAuthorizedRequest(uri);

		if (raw.Item2 != null)
			return null;
		else
			return raw.Item1;
    }

	public async Task<string> GetRemoteDatabaseInfoJson()
	{
        var uri = new Uri($"{ BaseURL }DatabaseInfo/.json");
        Tuple<string, TitledException> raw = await PostAuthorizedRequest(uri);

        if (raw.Item2 != null)
            return null;
        else
            return raw.Item1;
	}

	public async Task<string> GetAllLanguageJson()
	{
		var uri = new Uri($"{ BaseURL }Translations/.json");
        Tuple<string, TitledException> raw = await PostAuthorizedRequest(uri);

		if (raw.Item2 != null)
			return null;
		else
			return raw.Item1;
    }

	public async Task<string> GetLanguageCodeJson()
	{
        var uri = new Uri($"{ BaseURL }TranslationOptions/.json");
        Tuple<string, TitledException> raw = await PostAuthorizedRequest(uri);

		if (raw.Item2 != null)
			return null;
		else
			return raw.Item1;
    }

    //Unused
	private async Task<string> GetLanguage(TranslationOptionModel option)
    {
        var uri = new Uri($"{ BaseURL }Languages/{ option }/.json");
			Tuple<string, TitledException> raw = await PostAuthorizedRequest(uri);

        if (raw.Item2 != null)
            return null;
        else
            return raw.Item1;
	}
}
