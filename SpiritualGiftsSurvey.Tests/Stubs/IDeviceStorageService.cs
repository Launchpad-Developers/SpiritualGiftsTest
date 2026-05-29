namespace SpiritualGiftsSurvey.Services;

/// <summary>
/// Test stub for IDeviceStorageService to avoid MAUI dependencies in test project.
/// The actual implementation is in the main project.
/// </summary>
public interface IDeviceStorageService
{
    string GetDatabaseFileLocation();
    string GetDatabaseFolderLocation();
    string GetAbsoluteFilePath(string filePath);
    bool FileExists(string absolutePath);
    Stream ReadFile(string absolutePath);
    Stream CreateOrWriteFile(string absolutePath);
    void DeleteFile(string absoluteFilePath);
}
