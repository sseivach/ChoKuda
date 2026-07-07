namespace ChoKuda.Core.FileLibrary;

public sealed class AppEnvironmentPaths
{
    public AppEnvironmentPaths(string appDataPath, string userProfilePath)
    {
        if (string.IsNullOrWhiteSpace(appDataPath))
        {
            throw new ArgumentException("AppData path is required.", nameof(appDataPath));
        }

        if (string.IsNullOrWhiteSpace(userProfilePath))
        {
            throw new ArgumentException("User profile path is required.", nameof(userProfilePath));
        }

        AppDataPath = appDataPath;
        UserProfilePath = userProfilePath;
    }

    public string AppDataPath { get; }

    public string UserProfilePath { get; }

    public string AppSettingsFilePath =>
        StandardPaths.GetAppSettingsFilePath(AppDataPath);

    public string DefaultLibraryPath =>
        StandardPaths.GetDefaultLibraryPath(UserProfilePath);
}

