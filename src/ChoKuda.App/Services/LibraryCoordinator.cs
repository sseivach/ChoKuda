using System.IO;
using ChoKuda.Core.FileLibrary;

namespace ChoKuda.App.Services;

public sealed class LibraryCoordinator
{
    private readonly AppSettingsService _appSettingsService;
    private readonly FileLibraryService _fileLibraryService;

    public LibraryCoordinator(
        AppSettingsService appSettingsService,
        FileLibraryService fileLibraryService)
    {
        _appSettingsService = appSettingsService;
        _fileLibraryService = fileLibraryService;
    }

    public AppSettings LoadSettings() =>
        _appSettingsService.Load();

    public bool LibraryFolderExists(string libraryPath) =>
        Directory.Exists(libraryPath);

    public LibraryOperationResult SaveSettings(AppSettings appSettings)
    {
        try
        {
            _appSettingsService.Save(appSettings);
            return LibraryOperationResult.Success();
        }
        catch (Exception exception) when (IsRecoverableException(exception))
        {
            return LibraryOperationResult.Failure(exception.Message);
        }
    }

    public LibraryOperationResult OpenLibrary(
        string libraryPath,
        AppSettings appSettings)
    {
        try
        {
            _fileLibraryService.EnsureLibrary(libraryPath);
            appSettings.LibraryPath = libraryPath;
            _appSettingsService.Save(appSettings);

            return LibraryOperationResult.Success();
        }
        catch (Exception exception) when (IsRecoverableException(exception))
        {
            return LibraryOperationResult.Failure(exception.Message);
        }
    }

    public LibraryDataLoadResult LoadData(FileLibraryPaths paths)
    {
        try
        {
            var pointsResult = _fileLibraryService.LoadPoints(paths);
            var collectionsResult = _fileLibraryService.LoadCollections(paths);

            return LibraryDataLoadResult.Success(
                pointsResult.Items,
                pointsResult.Errors,
                collectionsResult.Items,
                collectionsResult.Errors);
        }
        catch (Exception exception) when (IsRecoverableException(exception))
        {
            return LibraryDataLoadResult.Failure(exception.Message);
        }
    }

    private static bool IsRecoverableException(Exception exception) =>
        exception is IOException
            or UnauthorizedAccessException
            or ArgumentException
            or NotSupportedException;
}

public sealed record LibraryOperationResult(
    bool IsSuccess,
    string? ErrorMessage)
{
    public static LibraryOperationResult Success() =>
        new(IsSuccess: true, ErrorMessage: null);

    public static LibraryOperationResult Failure(string errorMessage) =>
        new(IsSuccess: false, errorMessage);
}

public sealed record LibraryDataLoadResult(
    bool IsSuccess,
    IReadOnlyList<PointDocument> Points,
    IReadOnlyList<FileLibraryLoadError> PointErrors,
    IReadOnlyList<CollectionDocument> Collections,
    IReadOnlyList<FileLibraryLoadError> CollectionErrors,
    string? ErrorMessage)
{
    public static LibraryDataLoadResult Success(
        IReadOnlyList<PointDocument> points,
        IReadOnlyList<FileLibraryLoadError> pointErrors,
        IReadOnlyList<CollectionDocument> collections,
        IReadOnlyList<FileLibraryLoadError> collectionErrors) =>
        new(
            IsSuccess: true,
            Points: points,
            PointErrors: pointErrors,
            Collections: collections,
            CollectionErrors: collectionErrors,
            ErrorMessage: null);

    public static LibraryDataLoadResult Failure(string errorMessage) =>
        new(
            IsSuccess: false,
            Points: Array.Empty<PointDocument>(),
            PointErrors: Array.Empty<FileLibraryLoadError>(),
            Collections: Array.Empty<CollectionDocument>(),
            CollectionErrors: Array.Empty<FileLibraryLoadError>(),
            ErrorMessage: errorMessage);
}
