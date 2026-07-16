using ChoKuda.App.ViewModels;
using ChoKuda.Core.FileLibrary;

namespace ChoKuda.App.Tests.ViewModels;

public sealed class LibraryViewModelTests
{
    [Fact]
    public void SetNoLibrarySelectedResetsToSetupState()
    {
        var library = new LibraryViewModel();
        library.SetReady(Path.Combine("c:", "ChoKudaLibrary"));
        library.SetLoadFailed(Path.Combine("c:", "ChoKudaLibrary"), "Broken JSON.");

        library.SetNoLibrarySelected();

        Assert.False(library.IsReady);
        Assert.Null(library.Path);
        Assert.Equal("Choose ChoKuda library", library.SetupTitle);
        Assert.Empty(library.SetupErrors);
        Assert.Empty(library.LoadErrors);
        Assert.Null(library.GetCurrentPaths());
    }

    [Fact]
    public void SetMissingLibraryStoresRecoverableSetupError()
    {
        var library = new LibraryViewModel();
        var libraryPath = Path.Combine("c:", "missing");

        library.SetMissingLibrary(libraryPath);

        Assert.False(library.IsReady);
        Assert.Equal(libraryPath, library.Path);
        Assert.Equal("Library not found", library.SetupTitle);
        Assert.Equal([$"Saved library folder was not found: {libraryPath}"], library.SetupErrors);
        Assert.Empty(library.LoadErrors);
    }

    [Fact]
    public void SetReadyEnablesCurrentPathsAndClearsSetupErrors()
    {
        var library = new LibraryViewModel();
        var libraryPath = Path.Combine("c:", "ChoKudaLibrary");
        library.SetOpenFailed(libraryPath, "Access denied.");

        library.SetReady(libraryPath);

        var paths = library.GetCurrentPaths();
        Assert.True(library.IsReady);
        Assert.Equal(libraryPath, library.Path);
        Assert.Equal("Choose ChoKuda library", library.SetupTitle);
        Assert.Empty(library.SetupErrors);
        Assert.NotNull(paths);
        Assert.Equal(Path.Combine(libraryPath, "points"), paths.PointsPath);
    }

    [Fact]
    public void SetOpenFailedDisablesLibraryAndClearsLoadErrors()
    {
        var library = new LibraryViewModel();
        var libraryPath = Path.Combine("c:", "readonly");
        library.SetLoadFailed(libraryPath, "Old load error.");

        library.SetOpenFailed(libraryPath, "Access denied.");

        Assert.False(library.IsReady);
        Assert.Equal(libraryPath, library.Path);
        Assert.Equal("Library could not be opened", library.SetupTitle);
        Assert.Equal([$"Library folder could not be prepared: {libraryPath}. Reason: Access denied."], library.SetupErrors);
        Assert.Empty(library.LoadErrors);
    }

    [Fact]
    public void SetLoadErrorsFormatsPointAndCollectionErrors()
    {
        var library = new LibraryViewModel();

        library.SetLoadErrors(
            [new FileLibraryLoadError(Path.Combine("library", "points", "point.json"), "Point broke.")],
            [new FileLibraryLoadError(Path.Combine("library", "collections", "collection.json"), "Collection broke.")]);

        Assert.True(library.HasLoadErrors);
        Assert.Equal(
            [
                "Point JSON could not be loaded: point.json. Reason: Point broke.",
                "Collection JSON could not be loaded: collection.json. Reason: Collection broke.",
            ],
            library.LoadErrors);
    }

}
