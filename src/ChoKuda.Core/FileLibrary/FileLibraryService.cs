using System.Text.Json;

namespace ChoKuda.Core.FileLibrary;

public sealed class FileLibraryService
{
    private readonly JsonSerializerOptions _jsonOptions;

    public FileLibraryService()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }

    public FileLibraryPaths EnsureLibrary(string rootPath)
    {
        var paths = new FileLibraryPaths(rootPath);

        Directory.CreateDirectory(paths.RootPath);
        Directory.CreateDirectory(paths.PointsPath);
        Directory.CreateDirectory(paths.CollectionsPath);
        Directory.CreateDirectory(paths.PhotosPath);
        Directory.CreateDirectory(paths.FilesPath);

        if (!File.Exists(paths.SettingsFilePath))
        {
            SaveJson(paths.SettingsFilePath, LibrarySettings.CreateDefault());
        }

        return paths;
    }

    public void SavePoint(FileLibraryPaths paths, PointDocument point) =>
        SaveJson(paths.GetPointFilePath(point.Id), point);

    public void SaveCollection(FileLibraryPaths paths, CollectionDocument collection) =>
        SaveJson(paths.GetCollectionFilePath(collection.Id), collection);

    public FileLibraryLoadResult<PointDocument> LoadPoints(FileLibraryPaths paths) =>
        LoadDocuments<PointDocument>(paths.PointsPath);

    public FileLibraryLoadResult<CollectionDocument> LoadCollections(FileLibraryPaths paths) =>
        LoadDocuments<CollectionDocument>(paths.CollectionsPath);

    public T LoadJson<T>(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var value = JsonSerializer.Deserialize<T>(stream, _jsonOptions);

        if (value is null)
        {
            throw new InvalidDataException($"JSON file is empty or invalid: {filePath}");
        }

        return value;
    }

    public void SaveJson<T>(string filePath, T value)
    {
        var directoryPath = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var temporaryPath = $"{filePath}.tmp";
        using (var stream = File.Create(temporaryPath))
        {
            JsonSerializer.Serialize(stream, value, _jsonOptions);
        }

        File.Move(temporaryPath, filePath, overwrite: true);
    }

    private FileLibraryLoadResult<T> LoadDocuments<T>(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return new FileLibraryLoadResult<T>(Array.Empty<T>(), Array.Empty<FileLibraryLoadError>());
        }

        var items = new List<T>();
        var errors = new List<FileLibraryLoadError>();

        foreach (var filePath in Directory.EnumerateFiles(directoryPath, "*.json").OrderBy(path => path))
        {
            try
            {
                items.Add(LoadJson<T>(filePath));
            }
            catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException or InvalidDataException)
            {
                errors.Add(new FileLibraryLoadError(filePath, exception.Message));
            }
        }

        return new FileLibraryLoadResult<T>(items, errors);
    }
}

