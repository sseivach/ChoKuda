using ChoKuda.Core.FileLibrary;
using ChoKuda.Core.Points;

namespace ChoKuda.Core.Tests.Points;

public sealed class PointServiceTests
{
    [Fact]
    public void CreateDraftCreatesDefaultUnsavedPointAtCoordinates()
    {
        var service = CreateService();

        var draft = service.CreateDraft(36.255, -112.697);

        Assert.Equal(Guid.Empty, draft.Id);
        Assert.Equal("New point", draft.Title);
        Assert.Equal(36.255, draft.Latitude);
        Assert.Equal(-112.697, draft.Longitude);
    }

    [Fact]
    public void DefaultConstructorCreatesUsableService()
    {
        var service = new PointService();

        var draft = service.CreateDraft(1, 2);

        Assert.Equal("New point", draft.Title);
        Assert.Equal(1, draft.Latitude);
        Assert.Equal(2, draft.Longitude);
    }

    [Fact]
    public void CreatePointGeneratesIdNormalizesFieldsAndSavesJson()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var service = CreateService(fileLibrary);
        var collectionId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var draft = new PointDocument
        {
            Title = "  Havasu Falls  ",
            Latitude = 36.255,
            Longitude = -112.697,
            AddressRegion = "  Arizona  ",
            DescriptionText = "  Permit required.  ",
            CollectionIds = [collectionId],
            PrimaryCollectionId = collectionId,
            TagsText = "  #Waterfall   #Hike  ",
            Photos = ["waterfall__33333333-3333-3333-3333-333333333333.jpg"],
            Files = ["permit__44444444-4444-4444-4444-444444444444.pdf"],
        };

        var result = service.CreatePoint(paths, draft);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.NotNull(result.Point);
        var point = result.Point;
        Assert.NotEqual(Guid.Empty, point.Id);
        Assert.Equal("Havasu Falls", point.Title);
        Assert.Equal("Arizona", point.AddressRegion);
        Assert.Equal("Permit required.", point.DescriptionText);
        Assert.Equal("#waterfall #hike", point.TagsText);
        Assert.Equal([collectionId], point.CollectionIds);
        Assert.Equal(collectionId, point.PrimaryCollectionId);
        Assert.Equal(draft.Photos, point.Photos);
        Assert.Equal(draft.Files, point.Files);
        Assert.True(File.Exists(paths.GetPointFilePath(point.Id)));
    }

    [Fact]
    public void CreatePointClearsPrimaryCollectionWhenPointHasNoCollections()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var service = CreateService(fileLibrary);
        var draft = CreateValidPoint();
        draft.PrimaryCollectionId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var result = service.CreatePoint(paths, draft);

        Assert.NotNull(result.Point);
        var point = result.Point;
        Assert.Null(point.PrimaryCollectionId);
    }

    [Fact]
    public void CreatePointRejectsInvalidTitleCoordinatesAndTags()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var service = CreateService(fileLibrary);
        var draft = new PointDocument
        {
            Title = " ",
            Latitude = double.NaN,
            Longitude = 181,
            TagsText = "#rent car",
        };

        var result = service.CreatePoint(paths, draft);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Point);
        Assert.Contains(result.Errors, error => error.FieldName == PointService.TitleFieldName);
        Assert.Contains(result.Errors, error => error.FieldName == PointService.TagsFieldName);
        Assert.Contains(result.Errors, error => error.FieldName == PointService.GeneralFieldName);
        Assert.Empty(Directory.EnumerateFiles(paths.PointsPath));
    }

    [Fact]
    public void UpdatePointKeepsIdAndOverwritesExistingJson()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var service = CreateService(fileLibrary);
        var point = CreateValidPoint();
        fileLibrary.SavePoint(paths, point);
        point.Title = "  Updated title  ";
        point.TagsText = "  #SAFE  ";

        var result = service.UpdatePoint(paths, point);

        Assert.True(result.IsSuccess);
        var saved = fileLibrary.LoadJson<PointDocument>(paths.GetPointFilePath(point.Id));
        Assert.NotNull(result.Point);
        Assert.Equal(point.Id, result.Point.Id);
        Assert.Equal("Updated title", saved.Title);
        Assert.Equal("#safe", saved.TagsText);
    }

    [Fact]
    public void UpdatePointRejectsEmptyId()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var service = CreateService(fileLibrary);
        var point = CreateValidPoint();
        point.Id = Guid.Empty;

        var result = service.UpdatePoint(paths, point);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => error.Message == "Point id is required.");
    }

    [Fact]
    public void LoadPointReturnsNullWhenJsonIsMissing()
    {
        using var temp = TempDirectory.Create();
        var paths = new FileLibraryService().EnsureLibrary(temp.Path);
        var service = CreateService();

        var point = service.LoadPoint(paths, Guid.Parse("11111111-1111-1111-1111-111111111111"));

        Assert.Null(point);
    }

    [Fact]
    public void LoadPointReturnsSavedPoint()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var service = CreateService(fileLibrary);
        var saved = CreateValidPoint();
        fileLibrary.SavePoint(paths, saved);

        var loaded = service.LoadPoint(paths, saved.Id);

        Assert.NotNull(loaded);
        Assert.Equal(saved.Id, loaded.Id);
        Assert.Equal(saved.Title, loaded.Title);
    }

    [Fact]
    public void DeletePointRejectsEmptyIdAndMissingJson()
    {
        using var temp = TempDirectory.Create();
        var paths = new FileLibraryService().EnsureLibrary(temp.Path);
        var service = CreateService();

        var emptyIdResult = service.DeletePoint(paths, Guid.Empty);
        var missingResult = service.DeletePoint(paths, Guid.Parse("11111111-1111-1111-1111-111111111111"));

        Assert.False(emptyIdResult.IsSuccess);
        Assert.Contains("Point id is required.", emptyIdResult.Errors);
        Assert.False(missingResult.IsSuccess);
        Assert.Contains("Point JSON was not found.", missingResult.Errors);
    }

    [Fact]
    public void DeletePointRejectsBrokenJson()
    {
        using var temp = TempDirectory.Create();
        var paths = new FileLibraryService().EnsureLibrary(temp.Path);
        var service = CreateService();
        var pointId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        File.WriteAllText(paths.GetPointFilePath(pointId), "{ broken");

        var result = service.DeletePoint(paths, pointId);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => error.StartsWith("Point JSON could not be loaded:", StringComparison.Ordinal));
        Assert.True(File.Exists(paths.GetPointFilePath(pointId)));
    }

    [Fact]
    public void DeletePointDeletesLinkedFilesThenPointJson()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var fileSystem = new RecordingPointFileSystem();
        var service = CreateService(fileLibrary, fileSystem);
        var point = CreateValidPoint();
        point.Photos = ["photo.jpg"];
        point.Files = ["guide.pdf"];
        fileLibrary.SavePoint(paths, point);
        File.WriteAllText(Path.Combine(paths.PhotosPath, "photo.jpg"), "photo");
        File.WriteAllText(Path.Combine(paths.FilesPath, "guide.pdf"), "file");

        var result = service.DeletePoint(paths, point.Id);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.False(File.Exists(paths.GetPointFilePath(point.Id)));
        Assert.False(File.Exists(Path.Combine(paths.PhotosPath, "photo.jpg")));
        Assert.False(File.Exists(Path.Combine(paths.FilesPath, "guide.pdf")));
        Assert.Equal(
            [
                Path.Combine(paths.PhotosPath, "photo.jpg"),
                Path.Combine(paths.FilesPath, "guide.pdf"),
                paths.GetPointFilePath(point.Id),
            ],
            fileSystem.DeletedPaths);
    }

    [Fact]
    public void DeletePointTreatsMissingLinkedFilesAsAlreadyDeleted()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var service = CreateService(fileLibrary);
        var point = CreateValidPoint();
        point.Photos = ["missing.jpg"];
        point.Files = ["missing.pdf"];
        fileLibrary.SavePoint(paths, point);

        var result = service.DeletePoint(paths, point.Id);

        Assert.True(result.IsSuccess);
        Assert.False(File.Exists(paths.GetPointFilePath(point.Id)));
    }

    [Fact]
    public void DeletePointKeepsJsonWhenLinkedFileNameIsInvalid()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var service = CreateService(fileLibrary);
        var point = CreateValidPoint();
        point.Photos = ["", @"nested\photo.jpg"];
        point.Files = ["../guide.pdf"];
        fileLibrary.SavePoint(paths, point);

        var result = service.DeletePoint(paths, point.Id);

        Assert.False(result.IsSuccess);
        Assert.Contains("Linked photo has an empty file name.", result.Errors);
        Assert.Contains(@"Linked photo has an invalid file name: nested\photo.jpg", result.Errors);
        Assert.Contains("Linked file has an invalid file name: ../guide.pdf", result.Errors);
        Assert.True(File.Exists(paths.GetPointFilePath(point.Id)));
    }

    [Fact]
    public void DeletePointKeepsJsonWhenLinkedFileDeleteFails()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var fileSystem = new RecordingPointFileSystem();
        var service = CreateService(fileLibrary, fileSystem);
        var point = CreateValidPoint();
        point.Photos = ["photo.jpg"];
        fileLibrary.SavePoint(paths, point);
        var photoPath = Path.Combine(paths.PhotosPath, "photo.jpg");
        File.WriteAllText(photoPath, "photo");
        fileSystem.ThrowOnDelete.Add(photoPath);

        var result = service.DeletePoint(paths, point.Id);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => error.StartsWith("Linked photo could not be deleted:", StringComparison.Ordinal));
        Assert.True(File.Exists(paths.GetPointFilePath(point.Id)));
        Assert.True(File.Exists(photoPath));
    }

    [Fact]
    public void DeletePointReportsJsonDeleteFailureAfterLinkedFilesAreDeleted()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var fileSystem = new RecordingPointFileSystem();
        var service = CreateService(fileLibrary, fileSystem);
        var point = CreateValidPoint();
        fileLibrary.SavePoint(paths, point);
        fileSystem.ThrowOnDelete.Add(paths.GetPointFilePath(point.Id));

        var result = service.DeletePoint(paths, point.Id);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => error.StartsWith("Point JSON could not be deleted:", StringComparison.Ordinal));
        Assert.True(File.Exists(paths.GetPointFilePath(point.Id)));
    }

    [Fact]
    public void ResultFactoriesCreateExpectedSuccessAndFailureResults()
    {
        var point = CreateValidPoint();

        var saveSuccess = PointSaveResult.Success(point);
        var saveFailure = PointSaveResult.Failure([new PointSaveError("title", "Title is required.")]);
        var deleteSuccess = PointDeleteResult.Success();
        var deleteFailure = PointDeleteResult.Failure(["failed"]);

        Assert.True(saveSuccess.IsSuccess);
        Assert.Same(point, saveSuccess.Point);
        Assert.False(saveFailure.IsSuccess);
        Assert.Single(saveFailure.Errors);
        Assert.True(deleteSuccess.IsSuccess);
        Assert.Empty(deleteSuccess.Errors);
        Assert.False(deleteFailure.IsSuccess);
        Assert.Equal(["failed"], deleteFailure.Errors);
    }

    private static PointService CreateService(
        FileLibraryService? fileLibraryService = null,
        IPointFileSystem? fileSystem = null) =>
        new(fileLibraryService ?? new FileLibraryService(), fileSystem ?? new RecordingPointFileSystem());

    private static PointDocument CreateValidPoint() =>
        new()
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Title = "Havasu Falls",
            Latitude = 36.255,
            Longitude = -112.697,
            AddressRegion = "Arizona",
            DescriptionText = "Permit required.",
            TagsText = "#waterfall",
        };

    private sealed class RecordingPointFileSystem : IPointFileSystem
    {
        public List<string> DeletedPaths { get; } = [];

        public HashSet<string> ThrowOnDelete { get; } = [];

        public bool FileExists(string filePath) =>
            File.Exists(filePath);

        public void DeleteFile(string filePath)
        {
            if (ThrowOnDelete.Contains(filePath))
            {
                throw new IOException("Delete failed.");
            }

            File.Delete(filePath);
            DeletedPaths.Add(filePath);
        }
    }
}
