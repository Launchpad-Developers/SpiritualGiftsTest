using System.IO;

namespace SpiritualGiftsTest.Interfaces;

public interface IDeviceStorageService
{
    string GetAbsoluteFilePath(string filePath);
    bool FileExists(string absolutePath);
    Stream ReadFile(string absolutePath);
    Stream CreateOrWriteFile(string absolutePath);
    void DeleteFile(string absoluteFilePath);
    string GetDatabaseFileLocation();
    string GetDatabaseFolderLocation();
}

