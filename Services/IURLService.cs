using System.Threading.Tasks;

namespace SpiritualGiftsTest.Interfaces;

public interface IURLService
{
	Task<string> GetFullDatabaseJson();
	Task<string> GetRemoteDatabaseInfoJson();
	Task<string> GetAllLanguageJson();
	Task<string> GetLanguageCodeJson();
}
