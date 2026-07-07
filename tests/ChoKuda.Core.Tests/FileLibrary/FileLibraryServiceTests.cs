using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Tests.FileLibrary;

public sealed class FileLibraryServiceTests
{
    [Fact]
    public void EnsureLibraryCreatesExpectedStructureAndDefaultSettings()
    {
        using var temp = TempDirectory.Create();
        var service = new FileLibraryService();

        var paths = service.EnsureLibrary(temp.Path);
        var settings = service.LoadJson<LibrarySettings>(paths.SettingsFilePath);

        Assert.True(Directory.Exists(paths.RootPath));
        Assert.True(Directory.Exists(paths.PointsPath));
        Assert.True(Directory.Exists(paths.CollectionsPath));
        Assert.True(Directory.Exists(paths.PhotosPath));
        Assert.True(Directory.Exists(paths.FilesPath));
        Assert.Equal(1, settings.SchemaVersion);
        Assert.Equal("Stadia", settings.MapProvider);
    }

    [Fact]
    public void EnsureLibraryDoesNotOverwriteExistingSettings()
    {
        using var temp = TempDirectory.Create();
        var service = new FileLibraryService();
        var paths = new FileLibraryPaths(temp.Path);
        Directory.CreateDirectory(paths.RootPath);
        service.SaveJson(paths.SettingsFilePath, new LibrarySettings
        {
            SchemaVersion = 9,
            MapProvider = "Custom",
        });

        service.EnsureLibrary(temp.Path);
        var settings = service.LoadJson<LibrarySettings>(paths.SettingsFilePath);

        Assert.Equal(9, settings.SchemaVersion);
        Assert.Equal("Custom", settings.MapProvider);
    }

    [Fact]
    public void SaveAndLoadPointRoundTripsDocument()
    {
        using var temp = TempDirectory.Create();
        var service = new FileLibraryService();
        var paths = service.EnsureLibrary(temp.Path);
        var point = CreatePoint();

        service.SavePoint(paths, point);
        var result = service.LoadPoints(paths);

        var loadedPoint = Assert.Single(result.Items);
        Assert.False(result.HasErrors);
        Assert.Empty(result.Errors);
        Assert.Equal(point.Id, loadedPoint.Id);
        Assert.Equal(point.Title, loadedPoint.Title);
        Assert.Equal(point.Latitude, loadedPoint.Latitude);
        Assert.Equal(point.Longitude, loadedPoint.Longitude);
        Assert.Equal(point.AddressRegion, loadedPoint.AddressRegion);
        Assert.Equal(point.DescriptionText, loadedPoint.DescriptionText);
        Assert.Equal(point.CollectionIds, loadedPoint.CollectionIds);
        Assert.Equal(point.PrimaryCollectionId, loadedPoint.PrimaryCollectionId);
        Assert.Equal(point.TagsText, loadedPoint.TagsText);
        Assert.Equal(point.Photos, loadedPoint.Photos);
        Assert.Equal(point.Files, loadedPoint.Files);
        Assert.Equal(point.CreatedAt, loadedPoint.CreatedAt);
        Assert.Equal(point.UpdatedAt, loadedPoint.UpdatedAt);
    }

    [Fact]
    public void SaveAndLoadCollectionRoundTripsDocument()
    {
        using var temp = TempDirectory.Create();
        var service = new FileLibraryService();
        var paths = service.EnsureLibrary(temp.Path);
        var collection = CreateCollection();

        service.SaveCollection(paths, collection);
        var result = service.LoadCollections(paths);

        var loadedCollection = Assert.Single(result.Items);
        Assert.False(result.HasErrors);
        Assert.Empty(result.Errors);
        Assert.Equal(collection.Id, loadedCollection.Id);
        Assert.Equal(collection.Name, loadedCollection.Name);
        Assert.Equal(collection.IconId, loadedCollection.IconId);
        Assert.Equal(collection.Color, loadedCollection.Color);
        Assert.Equal(collection.DescriptionText, loadedCollection.DescriptionText);
        Assert.Equal(collection.CreatedAt, loadedCollection.CreatedAt);
        Assert.Equal(collection.UpdatedAt, loadedCollection.UpdatedAt);
    }

    [Fact]
    public void LoadDocumentsReturnsEmptyResultWhenDirectoryDoesNotExist()
    {
        using var temp = TempDirectory.Create();
        var service = new FileLibraryService();
        var paths = new FileLibraryPaths(temp.Path);

        var result = service.LoadPoints(paths);

        Assert.Empty(result.Items);
        Assert.Empty(result.Errors);
        Assert.False(result.HasErrors);
    }

    [Fact]
    public void LoadDocumentsReturnsItemsAndControlledErrorsForBrokenJson()
    {
        using var temp = TempDirectory.Create();
        var service = new FileLibraryService();
        var paths = service.EnsureLibrary(temp.Path);
        var point = CreatePoint();
        service.SavePoint(paths, point);
        var brokenPath = Path.Combine(paths.PointsPath, "broken.json");
        File.WriteAllText(brokenPath, "{ broken");

        var result = service.LoadPoints(paths);

        Assert.True(result.HasErrors);
        Assert.Single(result.Items);
        var error = Assert.Single(result.Errors);
        Assert.Equal(brokenPath, error.FilePath);
        Assert.False(string.IsNullOrWhiteSpace(error.Message));
    }

    [Fact]
    public void LoadJsonThrowsForNullJson()
    {
        using var temp = TempDirectory.Create();
        var service = new FileLibraryService();
        var filePath = Path.Combine(temp.Path, "null.json");
        File.WriteAllText(filePath, "null");

        var exception = Assert.Throws<InvalidDataException>(() =>
            service.LoadJson<PointDocument>(filePath));

        Assert.Contains("empty or invalid", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SaveJsonCanWriteFileWithoutDirectoryComponent()
    {
        using var temp = TempDirectory.Create();
        var previousDirectory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(temp.Path);

        try
        {
            var service = new FileLibraryService();
            service.SaveJson("appsettings.json", new AppSettings
            {
                LibraryPath = "library",
                StadiaApiKey = "key",
            });

            var loaded = service.LoadJson<AppSettings>("appsettings.json");
            Assert.Equal("library", loaded.LibraryPath);
            Assert.Equal("key", loaded.StadiaApiKey);
        }
        finally
        {
            Directory.SetCurrentDirectory(previousDirectory);
        }
    }

    [Fact]
    public void NewDocumentsHaveExpectedDefaults()
    {
        var point = new PointDocument();
        var collection = new CollectionDocument();
        var settings = new AppSettings();

        Assert.Equal(Guid.Empty, point.Id);
        Assert.Equal(string.Empty, point.Title);
        Assert.Equal(0, point.Latitude);
        Assert.Equal(0, point.Longitude);
        Assert.Equal(string.Empty, point.AddressRegion);
        Assert.Equal(string.Empty, point.DescriptionText);
        Assert.Empty(point.CollectionIds);
        Assert.Null(point.PrimaryCollectionId);
        Assert.Equal(string.Empty, point.TagsText);
        Assert.Empty(point.Photos);
        Assert.Empty(point.Files);
        Assert.Equal(default, point.CreatedAt);
        Assert.Equal(default, point.UpdatedAt);

        Assert.Equal(Guid.Empty, collection.Id);
        Assert.Equal(string.Empty, collection.Name);
        Assert.Equal(string.Empty, collection.IconId);
        Assert.Equal(string.Empty, collection.Color);
        Assert.Equal(string.Empty, collection.DescriptionText);
        Assert.Equal(default, collection.CreatedAt);
        Assert.Equal(default, collection.UpdatedAt);

        Assert.Null(settings.LibraryPath);
        Assert.Null(settings.StadiaApiKey);
    }

    private static PointDocument CreatePoint()
    {
        var collectionId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        return new PointDocument
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Title = "Havasu Falls",
            Latitude = 36.255,
            Longitude = -112.697,
            AddressRegion = "Arizona",
            DescriptionText = "Permit required.",
            CollectionIds = [collectionId],
            PrimaryCollectionId = collectionId,
            TagsText = "#waterfall #hike",
            Photos = ["waterfall__33333333-3333-3333-3333-333333333333.jpg"],
            Files = ["permit__44444444-4444-4444-4444-444444444444.pdf"],
            CreatedAt = DateTimeOffset.Parse("2026-07-07T10:00:00-04:00"),
            UpdatedAt = DateTimeOffset.Parse("2026-07-07T10:30:00-04:00"),
        };
    }

    private static CollectionDocument CreateCollection()
    {
        return new CollectionDocument
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Arizona",
            IconId = "circle",
            Color = "#ff0000",
            DescriptionText = "Arizona ideas.",
            CreatedAt = DateTimeOffset.Parse("2026-07-07T09:00:00-04:00"),
            UpdatedAt = DateTimeOffset.Parse("2026-07-07T09:30:00-04:00"),
        };
    }
}

