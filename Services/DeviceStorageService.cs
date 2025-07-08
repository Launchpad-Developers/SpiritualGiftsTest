using System.Diagnostics;

namespace SpiritualGiftsSurvey.Services;
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

public class DeviceStorageService : IDeviceStorageService
{
    public string GetAbsoluteFilePath(string filePath)
    {
        return Path.Combine(FileSystem.AppDataDirectory, filePath);
    }

    public bool FileExists(string absolutePath)
    {
        return File.Exists(absolutePath);
    }

    public Stream ReadFile(string absolutePath)
    {
        return File.OpenRead(absolutePath);
    }

    public Stream CreateOrWriteFile(string absolutePath)
    {
        // Open or create for read/write
        return File.Open(absolutePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
    }

    public void DeleteFile(string absoluteFilePath)
    {
        if (File.Exists(absoluteFilePath))
        {
            File.Delete(absoluteFilePath);
        }
    }

    public string GetDatabaseFileLocation()
    {
        string folderPath = GetDatabaseFolderLocation();

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.WriteLine($"***** DeviceStorageService created folder path");
        }

        string fullPath = Path.Combine(folderPath, "SpiritualGiftsSurvey.sqlite");

        Debug.WriteLine($"***** DeviceStorageService folderPath: {fullPath}");
        return fullPath;
    }

    public string GetDatabaseFolderLocation()
    {
        return FileSystem.AppDataDirectory;
    }
}

