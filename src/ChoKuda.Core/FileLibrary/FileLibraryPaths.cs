namespace ChoKuda.Core.FileLibrary;

public sealed class FileLibraryPaths
{
    public FileLibraryPaths(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentException("Library root path is required.", nameof(rootPath));
        }

        RootPath = rootPath;
    }

    public string RootPath { get; }

    public string SettingsFilePath => Path.Combine(RootPath, "settings.json");

    public string PointsPath => Path.Combine(RootPath, "points");

    public string CollectionsPath => Path.Combine(RootPath, "collections");

    public string PhotosPath => Path.Combine(RootPath, "photos");

    public string FilesPath => Path.Combine(RootPath, "files");

    public string GetPointFilePath(Guid pointId) =>
        Path.Combine(PointsPath, $"{pointId}.json");

    public string GetCollectionFilePath(Guid collectionId) =>
        Path.Combine(CollectionsPath, $"{collectionId}.json");
}

