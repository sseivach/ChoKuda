using ChoKuda.Core.FileLibrary;

namespace ChoKuda.App.ViewModels;

public sealed class LibraryViewModel
{
    private const string DefaultSetupTitle = "Choose ChoKuda library";
    private const string DefaultSetupMessage = "Create a new library in the default location or choose an existing library folder.";

    public bool IsReady { get; private set; }

    public string? Path { get; private set; }

    public string SetupTitle { get; private set; } = DefaultSetupTitle;

    public string SetupMessage { get; private set; } = DefaultSetupMessage;

    public IReadOnlyList<string> SetupErrors { get; private set; } = Array.Empty<string>();

    public IReadOnlyList<string> LoadErrors { get; private set; } = Array.Empty<string>();

    public bool HasLoadErrors =>
        LoadErrors.Count > 0;

    public void SetNoLibrarySelected()
    {
        IsReady = false;
        Path = null;
        SetupTitle = DefaultSetupTitle;
        SetupMessage = DefaultSetupMessage;
        SetupErrors = Array.Empty<string>();
        LoadErrors = Array.Empty<string>();
    }

    public void SetMissingLibrary(string libraryPath)
    {
        IsReady = false;
        Path = libraryPath;
        SetupTitle = "Library not found";
        SetupMessage = "The saved ChoKuda library folder is missing or unavailable.";
        SetupErrors =
        [
            $"Saved library folder was not found: {libraryPath}",
        ];
        LoadErrors = Array.Empty<string>();
    }

    public void SetReady(string libraryPath)
    {
        IsReady = true;
        Path = libraryPath;
        SetupTitle = DefaultSetupTitle;
        SetupMessage = DefaultSetupMessage;
        SetupErrors = Array.Empty<string>();
    }

    public void SetOpenFailed(string libraryPath, string message)
    {
        IsReady = false;
        Path = libraryPath;
        SetupTitle = "Library could not be opened";
        SetupMessage = "ChoKuda could not create or open the selected library folder.";
        SetupErrors =
        [
            $"Library folder could not be prepared: {libraryPath}. Reason: {message}",
        ];
        LoadErrors = Array.Empty<string>();
    }

    public void ClearLoadErrors()
    {
        LoadErrors = Array.Empty<string>();
    }

    public void SetLoadErrors(
        IEnumerable<FileLibraryLoadError> pointErrors,
        IEnumerable<FileLibraryLoadError> collectionErrors)
    {
        LoadErrors = FormatLoadErrors("Point", pointErrors)
            .Concat(FormatLoadErrors("Collection", collectionErrors))
            .ToArray();
    }

    public void SetLoadFailed(string libraryPath, string message)
    {
        LoadErrors =
        [
            $"Library data could not be loaded from {libraryPath}. Reason: {message}",
        ];
    }

    public FileLibraryPaths? GetCurrentPaths()
    {
        if (!IsReady || string.IsNullOrWhiteSpace(Path))
        {
            return null;
        }

        return new FileLibraryPaths(Path);
    }

    private static IEnumerable<string> FormatLoadErrors(
        string documentType,
        IEnumerable<FileLibraryLoadError> errors)
    {
        return errors.Select(error =>
            $"{documentType} JSON could not be loaded: {System.IO.Path.GetFileName(error.FilePath)}. Reason: {error.Message}");
    }
}
