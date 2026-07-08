using ChoKuda.Core.Collections;
using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Tests.Collections;

public sealed class CollectionServiceTests
{
    [Fact]
    public void CreateDraftCreatesDefaultCollectionDraft()
    {
        var service = CreateService();

        var draft = service.CreateDraft();

        Assert.Equal(Guid.Empty, draft.Id);
        Assert.Equal("New collection", draft.Name);
        Assert.Equal(CollectionService.DefaultIconId, draft.IconId);
        Assert.Equal(CollectionColor.DefaultColor, draft.Color);
    }

    [Fact]
    public void DefaultConstructorCreatesUsableService()
    {
        var service = new CollectionService();

        var draft = service.CreateDraft();

        Assert.Equal("New collection", draft.Name);
    }

    [Fact]
    public void CreateCollectionGeneratesIdNormalizesFieldsAndSavesJson()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var service = CreateService(fileLibrary);
        var draft = new CollectionDocument
        {
            Name = "  Arizona  ",
            IconId = "  camera-fill  ",
            Color = " #D94A38 ",
            DescriptionText = "  Winter ideas  ",
        };

        var result = service.CreateCollection(paths, draft, []);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.NotNull(result.Collection);
        Assert.NotEqual(Guid.Empty, result.Collection.Id);
        Assert.Equal("Arizona", result.Collection.Name);
        Assert.Equal("camera-fill", result.Collection.IconId);
        Assert.Equal("#d94a38", result.Collection.Color);
        Assert.Equal("Winter ideas", result.Collection.DescriptionText);
        Assert.True(File.Exists(paths.GetCollectionFilePath(result.Collection.Id)));
    }

    [Fact]
    public void CreateCollectionUsesDefaultIconWhenIconIsEmpty()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var service = CreateService(fileLibrary);
        var draft = CreateValidCollection();
        draft.IconId = " ";

        var result = service.CreateCollection(paths, draft, []);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Collection);
        Assert.Equal(CollectionService.DefaultIconId, result.Collection.IconId);
    }

    [Fact]
    public void CreateCollectionRejectsEmptyNameInvalidColorAndDuplicateName()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var service = CreateService(fileLibrary);
        var existing = CreateValidCollection();
        existing.Name = "Arizona";
        var draft = new CollectionDocument
        {
            Name = "  arizona  ",
            IconId = "geo-alt-fill",
            Color = "#bad",
        };

        var result = service.CreateCollection(paths, draft, [existing]);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Collection);
        Assert.Contains(result.Errors, error => error.FieldName == CollectionService.NameFieldName && error.Message == "Collection name already exists.");
        Assert.Contains(result.Errors, error => error.FieldName == CollectionService.ColorFieldName);
        Assert.Empty(Directory.EnumerateFiles(paths.CollectionsPath));
    }

    [Fact]
    public void CreateCollectionRejectsWhitespaceName()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var service = CreateService(fileLibrary);
        var draft = CreateValidCollection();
        draft.Name = " ";

        var result = service.CreateCollection(paths, draft, []);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => error.FieldName == CollectionService.NameFieldName && error.Message == "Name is required.");
    }

    [Fact]
    public void UpdateCollectionKeepsIdAndAllowsSameNameForSameCollection()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var service = CreateService(fileLibrary);
        var collection = CreateValidCollection();
        fileLibrary.SaveCollection(paths, collection);
        collection.Name = "  Updated  ";

        var result = service.UpdateCollection(paths, collection, [collection]);

        Assert.True(result.IsSuccess);
        var saved = fileLibrary.LoadJson<CollectionDocument>(paths.GetCollectionFilePath(collection.Id));
        Assert.NotNull(result.Collection);
        Assert.Equal(collection.Id, result.Collection.Id);
        Assert.Equal("Updated", saved.Name);
    }

    [Fact]
    public void UpdateCollectionRejectsEmptyId()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var service = CreateService(fileLibrary);
        var collection = CreateValidCollection();
        collection.Id = Guid.Empty;

        var result = service.UpdateCollection(paths, collection, []);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => error.FieldName == CollectionService.GeneralFieldName);
    }

    [Fact]
    public void DeleteCollectionRejectsEmptyIdAndMissingJson()
    {
        using var temp = TempDirectory.Create();
        var paths = new FileLibraryService().EnsureLibrary(temp.Path);
        var service = CreateService();

        var emptyIdResult = service.DeleteCollection(paths, Guid.Empty);
        var missingResult = service.DeleteCollection(paths, Guid.Parse("11111111-1111-1111-1111-111111111111"));

        Assert.False(emptyIdResult.IsSuccess);
        Assert.Contains("Collection id is required.", emptyIdResult.Errors);
        Assert.False(missingResult.IsSuccess);
        Assert.Contains("Collection JSON was not found.", missingResult.Errors);
    }

    [Fact]
    public void DeleteCollectionRejectsBrokenCollectionOrPointJsonBeforeChangingFiles()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var service = CreateService(fileLibrary);
        var collection = CreateValidCollection();
        fileLibrary.SaveCollection(paths, collection);
        File.WriteAllText(Path.Combine(paths.PointsPath, "broken.json"), "{ broken");

        var result = service.DeleteCollection(paths, collection.Id);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => error.StartsWith("Point JSON could not be loaded:", StringComparison.Ordinal));
        Assert.True(File.Exists(paths.GetCollectionFilePath(collection.Id)));
    }

    [Fact]
    public void DeleteCollectionRejectsBrokenCollectionJsonBeforeChangingFiles()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var service = CreateService(fileLibrary);
        var collection = CreateValidCollection();
        fileLibrary.SaveCollection(paths, collection);
        File.WriteAllText(Path.Combine(paths.CollectionsPath, "broken.json"), "{ broken");

        var result = service.DeleteCollection(paths, collection.Id);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => error.StartsWith("Collection JSON could not be loaded:", StringComparison.Ordinal));
        Assert.True(File.Exists(paths.GetCollectionFilePath(collection.Id)));
    }

    [Fact]
    public void DeleteCollectionUpdatesLinkedPointsThenDeletesCollectionJson()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var fileSystem = new RecordingCollectionFileSystem();
        var service = CreateService(fileLibrary, fileSystem);
        var arizona = CreateValidCollection();
        arizona.Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        arizona.Name = "Arizona";
        var summer = CreateValidCollection();
        summer.Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        summer.Name = "Summer";
        fileLibrary.SaveCollection(paths, arizona);
        fileLibrary.SaveCollection(paths, summer);
        var point = CreatePoint(arizona.Id, summer.Id);
        var unrelatedPoint = CreatePoint(Guid.Parse("33333333-3333-3333-3333-333333333333"));
        unrelatedPoint.Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        fileLibrary.SavePoint(paths, point);
        fileLibrary.SavePoint(paths, unrelatedPoint);

        var result = service.DeleteCollection(paths, arizona.Id);

        Assert.True(result.IsSuccess);
        Assert.False(File.Exists(paths.GetCollectionFilePath(arizona.Id)));
        var updatedPoint = fileLibrary.LoadJson<PointDocument>(paths.GetPointFilePath(point.Id));
        Assert.Equal([summer.Id], updatedPoint.CollectionIds);
        Assert.Equal(summer.Id, updatedPoint.PrimaryCollectionId);
        var unchangedPoint = fileLibrary.LoadJson<PointDocument>(paths.GetPointFilePath(unrelatedPoint.Id));
        Assert.Equal(unrelatedPoint.CollectionIds, unchangedPoint.CollectionIds);
        Assert.Equal([paths.GetCollectionFilePath(arizona.Id)], fileSystem.DeletedPaths);
    }

    [Fact]
    public void DeleteCollectionReportsPointUpdateFailure()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var service = CreateService(fileLibrary);
        var collection = CreateValidCollection();
        fileLibrary.SaveCollection(paths, collection);
        var point = CreatePoint(collection.Id);
        fileLibrary.SavePoint(paths, point);

        using var lockedPoint = File.Open(
            paths.GetPointFilePath(point.Id),
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        var result = service.DeleteCollection(paths, collection.Id);

        Assert.False(result.IsSuccess);
        Assert.Contains($"Cannot delete collection \"{collection.Name}\".", result.Errors);
        Assert.Contains($"Point \"{point.Title}\" could not be updated.", result.Errors);
        Assert.Contains(result.Errors, error => error.StartsWith("Reason:", StringComparison.Ordinal));
        Assert.Contains(result.Errors, error => error.StartsWith("Fix:", StringComparison.Ordinal));
    }

    [Fact]
    public void DeleteCollectionUsesIdAsNameWhenCollectionDocumentHasDifferentId()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var fileSystem = new RecordingCollectionFileSystem();
        var service = CreateService(fileLibrary, fileSystem);
        var requestedId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var storedCollection = CreateValidCollection();
        storedCollection.Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        fileLibrary.SaveJson(paths.GetCollectionFilePath(requestedId), storedCollection);

        var result = service.DeleteCollection(paths, requestedId);

        Assert.True(result.IsSuccess);
        Assert.Equal([paths.GetCollectionFilePath(requestedId)], fileSystem.DeletedPaths);
    }

    [Fact]
    public void DeleteCollectionSetsPrimaryCollectionToNullWhenNoCollectionsRemain()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var service = CreateService(fileLibrary);
        var collection = CreateValidCollection();
        fileLibrary.SaveCollection(paths, collection);
        var point = CreatePoint(collection.Id);
        fileLibrary.SavePoint(paths, point);

        var result = service.DeleteCollection(paths, collection.Id);

        Assert.True(result.IsSuccess);
        var updatedPoint = fileLibrary.LoadJson<PointDocument>(paths.GetPointFilePath(point.Id));
        Assert.Empty(updatedPoint.CollectionIds);
        Assert.Null(updatedPoint.PrimaryCollectionId);
    }

    [Fact]
    public void DeleteCollectionReportsCollectionDeleteFailure()
    {
        using var temp = TempDirectory.Create();
        var fileLibrary = new FileLibraryService();
        var paths = fileLibrary.EnsureLibrary(temp.Path);
        var fileSystem = new RecordingCollectionFileSystem();
        var service = CreateService(fileLibrary, fileSystem);
        var collection = CreateValidCollection();
        fileLibrary.SaveCollection(paths, collection);
        fileSystem.ThrowOnDelete.Add(paths.GetCollectionFilePath(collection.Id));

        var result = service.DeleteCollection(paths, collection.Id);

        Assert.False(result.IsSuccess);
        Assert.Contains($"Cannot delete collection \"{collection.Name}\".", result.Errors);
        Assert.Contains(result.Errors, error => error.StartsWith("Reason:", StringComparison.Ordinal));
        Assert.True(File.Exists(paths.GetCollectionFilePath(collection.Id)));
    }

    [Fact]
    public void ResultFactoriesCreateExpectedResults()
    {
        var collection = CreateValidCollection();

        var saveSuccess = CollectionSaveResult.Success(collection);
        var saveFailure = CollectionSaveResult.Failure([new CollectionSaveError("name", "Name is required.")]);
        var deleteSuccess = CollectionDeleteResult.Success();
        var deleteFailure = CollectionDeleteResult.Failure(["failed"]);

        Assert.True(saveSuccess.IsSuccess);
        Assert.Same(collection, saveSuccess.Collection);
        Assert.False(saveFailure.IsSuccess);
        Assert.Single(saveFailure.Errors);
        Assert.True(deleteSuccess.IsSuccess);
        Assert.Empty(deleteSuccess.Errors);
        Assert.False(deleteFailure.IsSuccess);
        Assert.Equal(["failed"], deleteFailure.Errors);
    }

    private static CollectionService CreateService(
        FileLibraryService? fileLibraryService = null,
        ICollectionFileSystem? fileSystem = null) =>
        new(fileLibraryService ?? new FileLibraryService(), fileSystem ?? new RecordingCollectionFileSystem());

    private static CollectionDocument CreateValidCollection() =>
        new()
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Name = "Arizona",
            IconId = "geo-alt-fill",
            Color = "#ff0000",
            DescriptionText = "Ideas.",
        };

    private static PointDocument CreatePoint(params Guid[] collectionIds) =>
        new()
        {
            Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            Title = "Havasu Falls",
            Latitude = 36.255,
            Longitude = -112.697,
            CollectionIds = collectionIds.ToList(),
            PrimaryCollectionId = collectionIds.FirstOrDefault() == Guid.Empty
                ? null
                : collectionIds.FirstOrDefault(),
        };

    private sealed class RecordingCollectionFileSystem : ICollectionFileSystem
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
