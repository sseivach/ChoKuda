namespace ChoKuda.Core.FileLibrary;

public static class StandardPaths
{
    public static string GetDefaultLibraryPath(string userProfilePath)
    {
        if (string.IsNullOrWhiteSpace(userProfilePath))
        {
            throw new ArgumentException("User profile path is required.", nameof(userProfilePath));
        }

        return Path.Combine(userProfilePath, "Documents", "ChoKudaLibrary");
    }

    public static string GetAppSettingsFilePath(string appDataPath)
    {
        if (string.IsNullOrWhiteSpace(appDataPath))
        {
            throw new ArgumentException("AppData path is required.", nameof(appDataPath));
        }

        return Path.Combine(appDataPath, "ChoKuda", "appsettings.json");
    }
}

