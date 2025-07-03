using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpiritualGiftsTest.Interfaces;
using SpiritualGiftsTest.Models;
using SQLite;
using System.Text.RegularExpressions;

namespace SpiritualGiftsTest.Services;

public class DatabaseService : IDatabaseService
{
	private IDeviceStorageService _deviceStorageService { get; }
    private IURLService _urlService { get; }
	public DatabaseService(IDeviceStorageService deviceStorageService, 
	                       IURLService uRLService)
    {
		_deviceStorageService = deviceStorageService;
            _urlService = uRLService;
    }

    private string _path { get { return _deviceStorageService.GetDatabaseFileLocation(); } }
        
    public async Task<TranslationModel> GetTranslationForCode(string code)
	{
		var good2Go = await CheckDatabase(false);
		if (!good2Go)
			return null;

		var translation = new TranslationModel();
        using (var conn = new SQLiteConnection(_path))
		{
            translation = conn.Table<TranslationModel>().Where(x => x.Code == code).FirstOrDefault();
            conn.Dispose();
        }

		return translation;
    }

	public IEnumerable<TranslationOptionModel> GetCurrentTranslationOptions(string languageCode)
    {
		var options = new List<TranslationOptionModel>();
            using (var conn = new SQLiteConnection(_path))
		{
			options = conn.Table<TranslationOptionModel>().Where(x => x.Code == languageCode).ToList();
			conn.Dispose();
        }
   
		return options;
	}

	private async Task<bool> CheckDatabase(bool deleteDb = false)
    {
		//This should only be TRUE for testing purposes
		if (deleteDb)
            _deviceStorageService.DeleteFile(_path);
			
		bool refresh = false;
		if (!File.Exists(_path))
		{
            refresh = true;
		}
		else
		{
            var conn = new SQLiteConnection(_path);
			var localVersion = conn.Table<DatabaseInfoModel>().FirstOrDefault();
			conn.Dispose();
			conn = null;

			if (localVersion == null)
			{
                    refresh = true;
			}
			else
			{
				var json = await _urlService.GetRemoteDatabaseInfoJson();
				if (!string.IsNullOrEmpty(json))
				{
					var latestVersion = JsonConvert.DeserializeObject<DatabaseInfoModel>(json);
                        if (latestVersion.Version > localVersion.Version)
                            refresh = true;
				}
			}    
		}
                
        if (refresh)
		{
			_deviceStorageService.DeleteFile(_path);
			CreateDatabase();

			var json = await _urlService.GetFullDatabaseJson();
			if (string.IsNullOrEmpty(json))
				return false;

			var tuple = ParseDatabaseJson(json);
			if (tuple == null)
				return false;
			
			PopulateDatabase(tuple);

			return true;
		}

		return true;
    }

    private void CreateDatabase()
	{
            using (var conn = new SQLiteConnection(_path))
		{
                conn.CreateTable<TranslationOptionModel>();
                conn.CreateTable<TranslationModel>();
                conn.CreateTable<DatabaseInfoModel>();

                conn.Dispose();
            }

            //GC.Collect(); //Possibly overkill
	}

	private Tuple<DatabaseInfoModel, Translations, TranslationOptions> ParseDatabaseJson(string json)
	{
            string dbKey = "\"DatabaseInfo\":";
            string optionsKey = ",\"TranslationOptions\"";
		string translationsKey = ",\"Translations\":";

            //DatabaseInfo
            int startIndex = json.IndexOf(dbKey, StringComparison.CurrentCulture) + dbKey.Length;
		int endIndex = json.IndexOf(optionsKey, StringComparison.CurrentCulture);

		string dbJson = json.Substring(startIndex, endIndex - startIndex);
            var dbInfo = ParseDatabaseInfo(dbJson);
		if (dbInfo == null)
			return null;

            //Languages
		startIndex = json.IndexOf(translationsKey, StringComparison.CurrentCulture) + translationsKey.Length;
		endIndex = json.Length - 1;

		string langJson = json.Substring(startIndex, endIndex - startIndex);
            var languages = ParseAllLanguages(langJson);
		if (languages == null)
                return null;
		
            //Options
		startIndex = json.IndexOf(optionsKey, StringComparison.CurrentCulture) + optionsKey.Length;
		endIndex = json.IndexOf(translationsKey, StringComparison.CurrentCulture);

		string optionsJson = json.Substring(startIndex, endIndex - startIndex);
            var options = ParseLanguageCodes(optionsJson);
		if (options == null)
                return null;
		
            return new Tuple<DatabaseInfoModel, Translations, TranslationOptions>(dbInfo, languages, options);
	}

	private DatabaseInfoModel ParseDatabaseInfo(string json)
	{
            var info = JsonConvert.DeserializeObject<DatabaseInfoModel>(json);
            return info;
	}

	private Translations ParseAllLanguages(string json)
	{
            Regex rgx = new Regex("\"[a-zA-Z]+\":{");
            string rawArray = rgx.Replace(json, "{");
            rawArray = "[" + rawArray.Remove(0, 1);
            rawArray = rawArray.Remove(rawArray.Length - 1, 1) + "]";

            var rawTranslations = JsonConvert.DeserializeObject<JArray>(rawArray);
            var translations = new Translations();
            foreach (var t in rawTranslations)
            {
			var filtered = t.ToString().Replace("\\\\\\", "\\");
                var translation = JsonConvert.DeserializeObject<TranslationModel>(filtered);
                translations.TranslationCollection.Add(translation);
            }
            return translations;
	}

	private TranslationOptions ParseLanguageCodes(string json)
	{
		//Ugliest hack of my career...so far
		json = json.Remove(0, 2);
		json = json.Remove(json.Length - 2, 2);
   
		Regex rgx = new Regex("},");
		var rawArray = rgx.Split(json);
		var options = new TranslationOptions();

		foreach (var s in rawArray)
		{
			string match = "\":{";
			int startIndex = 1;
			int endIndex = s.IndexOf(match, StringComparison.CurrentCulture);
			var code = s.Substring(startIndex, endIndex - startIndex);
			var rawCodes = s.Remove(0, code.Length + 1 + match.Length);
                
			Regex optionRgx = new Regex(",");
			var pairs = optionRgx.Split(rawCodes);
                foreach (var pair in pairs)
			{
				var cleanPair = pair.Replace("\"", "");
				endIndex = cleanPair.IndexOf(":", StringComparison.CurrentCulture);
				var langOpt = cleanPair.Substring(0, endIndex);
				var trans = cleanPair.Substring(endIndex + 1, cleanPair.Length - endIndex - 1);
				//Console.WriteLine($"{code}\t\t{langOpt}\t\t{trans}");
				options.TranslationOptionCollection.Add(new TranslationOptionModel
				{
                        Code = code,
                        CodeOption = langOpt,
                        CodeOptionTranslation = trans,
                        Selected = false
				});
			}
		}

		return options;
	}

        private bool PopulateDatabase(Tuple<DatabaseInfoModel, Translations, TranslationOptions> data)
	{
		int i, j, k;
		using (var conn = new SQLiteConnection(_path))
		{
                conn.BeginTransaction();

                i = conn.Insert(data.Item1);
                j = conn.InsertAll(data.Item2.TranslationCollection);
                k = conn.InsertAll(data.Item3.TranslationOptionCollection);

                conn.Commit();    
                conn.Dispose();    
		}

		//GC.Collect(); //Possibly overkill

		return i > 0 && j > 0 && k > 0;
	}
}
