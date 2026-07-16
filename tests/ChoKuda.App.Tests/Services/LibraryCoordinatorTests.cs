using ChoKuda.App.Services;
using ChoKuda.Core.FileLibrary;

namespace ChoKuda.App.Tests.Services;

public sealed class LibraryCoordinatorTests
{
    [Fact]
    public void OpenLibraryEnsuresLibraryAndSavesSelectedPath()
    {
        var directoryPath = CreateTempDirectoryPath();
        var appSettingsFilePath = Path.Combine(CreateTempDirectoryPath(), "appsettings.json");
        var appSettingsService = new AppSettingsService(appSettingsFilePath);
        var coordinator = new LibraryCoordinator(appSettingsService, new FileLibraryService());
        var appSettings = new AppSettings { StadiaApiKey = "stadia-key" };

        try
        {
            var result = coordinator.OpenLibrary(directoryPath, appSettings);

            Assert.True(result.IsSuccess);
            Assert.Null(result.ErrorMessage);
            Assert.Equal(directoryPath, appSettings.LibraryPath);
            Assert.True(Directory.Exists(Path.Combine(directoryPath, "points")));
            Assert.True(Directory.Exists(Path.Combine(directoryPath, "collections")));
            var savedSettings = appSettingsService.Load();
            Assert.Equal(directoryPath, savedSettings.LibraryPath);
            Assert.Equal("stadia-key", savedSettings.StadiaApiKey);
        }
        finally
        {
            DeleteIfExists(directoryPath);
            DeleteIfExists(Path.GetDirectoryName(appSettingsFilePath));
        }
    }

    [Fact]
    public void OpenLibraryReturnsFailureForInvalidPathAndDoesNotMutateSettings()
    {
        using var directory = CreateTempDirectory();
        var coordinator = CreateCoordinator(directory.Path);
        var appSettings = new AppSettings { LibraryPath = "old-path" };

        var result = coordinator.OpenLibrary("   ", appSettings);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Equal("old-path", appSettings.LibraryPath);
    }

    [Fact]
    public void SaveSettingsPersistsAppSettings()
    {
        using var directory = CreateTempDirectory();
        var appSettingsService = new AppSettingsService(Path.Combine(directory.Path, "appsettings.json"));
        var coordinator = new LibraryCoordinator(appSettingsService, new FileLibraryService());

        var result = coordinator.SaveSettings(new AppSettings
        {
            LibraryPath = "library",
            StadiaApiKey = "key",
        });

        Assert.True(result.IsSuccess);
        var savedSettings = appSettingsService.Load();
        Assert.Equal("library", savedSettings.LibraryPath);
        Assert.Equal("key", savedSettings.StadiaApiKey);
    }

    [Fact]
    public void LoadDataReturnsDocumentsAndControlledJsonErrors()
    {
        using var directory = CreateTempDirectory();
        var fileLibraryService = new FileLibraryService();
        var coordinator = new LibraryCoordinator(
            new AppSettingsService(Path.Combine(directory.Path, "appsettings.json")),
            fileLibraryService);
        var paths = fileLibraryService.EnsureLibrary(Path.Combine(directory.Path, "library"));
        var point = new PointDocument
        {
            Id = Guid.NewGuid(),
            Title = "Saved point",
            Latitude = 1,
            Longitude = 2,
        };
        var collection = new CollectionDocument
        {
            Id = Guid.NewGuid(),
            Name = "Saved collection",
            Color = "#ff0000",
            IconId = "sun-fill",
        };
        fileLibraryService.SavePoint(paths, point);
        fileLibraryService.SaveCollection(paths, collection);
        File.WriteAllText(Path.Combine(paths.PointsPath, "broken-point.json"), "{");
        File.WriteAllText(Path.Combine(paths.CollectionsPath, "broken-collection.json"), "{");

        var result = coordinator.LoadData(paths);

        Assert.True(result.IsSuccess);
        Assert.Equal([point.Id], result.Points.Select(loadedPoint => loadedPoint.Id));
        Assert.Equal([collection.Id], result.Collections.Select(loadedCollection => loadedCollection.Id));
        Assert.Single(result.PointErrors);
        Assert.Single(result.CollectionErrors);
    }

    [Fact]
    public void LibraryFolderExistsChecksDirectoryPresence()
    {
        using var directory = CreateTempDirectory();
        var coordinator = CreateCoordinator(directory.Path);

        Assert.True(coordinator.LibraryFolderExists(directory.Path));
        Assert.False(coordinator.LibraryFolderExists(Path.Combine(directory.Path, "missing")));
    }

    private static LibraryCoordinator CreateCoordinator(string directoryPath) =>
        new(
            new AppSettingsService(Path.Combine(directoryPath, "appsettings.json")),
            new FileLibraryService());

    private static TemporaryDirectory CreateTempDirectory() =>
        new();

    private static string CreateTempDirectoryPath() =>
        Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    private static void DeleteIfExists(string? path)
    {
        if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = CreateTempDirectoryPath();
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            DeleteIfExists(Path);
        }
    }
}
