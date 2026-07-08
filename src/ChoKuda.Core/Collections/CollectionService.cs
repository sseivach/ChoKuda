using ChoKuda.Core.Domain;
using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Collections;

public sealed class CollectionService
{
    public const string NameFieldName = "name";
    public const string ColorFieldName = "color";
    public const string GeneralFieldName = "general";
    public const string DefaultIconId = "geo-alt-fill";

    private readonly FileLibraryService _fileLibraryService;
    private readonly ICollectionFileSystem _fileSystem;

    public CollectionService()
        : this(new FileLibraryService(), new PhysicalCollectionFileSystem())
    {
    }

    public CollectionService(
        FileLibraryService fileLibraryService,
        ICollectionFileSystem fileSystem)
    {
        _fileLibraryService = fileLibraryService;
        _fileSystem = fileSystem;
    }

    public CollectionDocument CreateDraft() =>
        new()
        {
            Name = "New collection",
            IconId = DefaultIconId,
            Color = CollectionColor.DefaultColor,
        };

    public CollectionSaveResult CreateCollection(
        FileLibraryPaths paths,
        CollectionDocument draft,
        IReadOnlyCollection<CollectionDocument> existingCollections)
    {
        var collection = CopyAndNormalize(draft);
        collection.Id = Guid.NewGuid();

        return SaveValidatedCollection(paths, collection, existingCollections);
    }

    public CollectionSaveResult UpdateCollection(
        FileLibraryPaths paths,
        CollectionDocument collection,
        IReadOnlyCollection<CollectionDocument> existingCollections)
    {
        var normalizedCollection = CopyAndNormalize(collection);

        return SaveValidatedCollection(paths, normalizedCollection, existingCollections);
    }

    public CollectionDeleteResult DeleteCollection(FileLibraryPaths paths, Guid collectionId)
    {
        if (collectionId == Guid.Empty)
        {
            return CollectionDeleteResult.Failure(["Collection id is required."]);
        }

        var collectionPath = paths.GetCollectionFilePath(collectionId);

        if (!_fileSystem.FileExists(collectionPath))
        {
            return CollectionDeleteResult.Failure(["Collection JSON was not found."]);
        }

        var collectionsResult = _fileLibraryService.LoadCollections(paths);
        var pointsResult = _fileLibraryService.LoadPoints(paths);
        var loadErrors = BuildLoadErrors(collectionsResult.Errors, pointsResult.Errors);

        if (loadErrors.Count > 0)
        {
            return CollectionDeleteResult.Failure(loadErrors);
        }

        var collectionName = collectionsResult.Items
            .FirstOrDefault(collection => collection.Id == collectionId)
            ?.Name ?? collectionId.ToString();
        var remainingCollections = collectionsResult.Items
            .Where(collection => collection.Id != collectionId)
            .Select(collection => new CollectionSummary(collection.Id, collection.Name))
            .ToArray();
        var pointsToUpdate = PreparePointUpdates(pointsResult.Items, collectionId, remainingCollections);

        foreach (var point in pointsToUpdate)
        {
            try
            {
                _fileLibraryService.SavePoint(paths, point);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                return CollectionDeleteResult.Failure(BuildPointUpdateFailure(collectionName, point.Title, exception.Message));
            }
        }

        try
        {
            _fileSystem.DeleteFile(collectionPath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return CollectionDeleteResult.Failure(BuildCollectionDeleteFailure(collectionName, exception.Message));
        }

        return CollectionDeleteResult.Success();
    }

    private CollectionSaveResult SaveValidatedCollection(
        FileLibraryPaths paths,
        CollectionDocument collection,
        IReadOnlyCollection<CollectionDocument> existingCollections)
    {
        var errors = ValidateCollection(collection, existingCollections);

        if (errors.Count > 0)
        {
            return CollectionSaveResult.Failure(errors);
        }

        _fileLibraryService.SaveCollection(paths, collection);

        return CollectionSaveResult.Success(collection);
    }

    private static IReadOnlyList<CollectionSaveError> ValidateCollection(
        CollectionDocument collection,
        IReadOnlyCollection<CollectionDocument> existingCollections)
    {
        var errors = new List<CollectionSaveError>();

        if (collection.Id == Guid.Empty)
        {
            errors.Add(new CollectionSaveError(GeneralFieldName, "Collection id is required."));
        }

        if (string.IsNullOrWhiteSpace(collection.Name))
        {
            errors.Add(new CollectionSaveError(NameFieldName, "Name is required."));
        }

        if (CollectionColor.Normalize(collection.Color) is null)
        {
            errors.Add(new CollectionSaveError(ColorFieldName, "Color must use #rrggbb format."));
        }

        var normalizedName = NormalizeName(collection.Name);
        var hasDuplicate = existingCollections.Any(existing =>
            existing.Id != collection.Id &&
            NormalizeName(existing.Name) == normalizedName);

        if (hasDuplicate)
        {
            errors.Add(new CollectionSaveError(NameFieldName, "Collection name already exists."));
        }

        return errors;
    }

    private static CollectionDocument CopyAndNormalize(CollectionDocument collection) =>
        new()
        {
            Id = collection.Id,
            Name = collection.Name.Trim(),
            IconId = string.IsNullOrWhiteSpace(collection.IconId)
                ? DefaultIconId
                : collection.IconId.Trim(),
            Color = CollectionColor.Normalize(collection.Color) ?? collection.Color.Trim().ToLowerInvariant(),
            DescriptionText = collection.DescriptionText.Trim(),
        };

    private static IReadOnlyList<PointDocument> PreparePointUpdates(
        IEnumerable<PointDocument> points,
        Guid removedCollectionId,
        IReadOnlyCollection<CollectionSummary> remainingCollections)
    {
        var updates = new List<PointDocument>();

        foreach (var point in points)
        {
            if (!point.CollectionIds.Contains(removedCollectionId))
            {
                continue;
            }

            var remainingPointCollectionIds = point.CollectionIds
                .Where(collectionId => collectionId != removedCollectionId)
                .Distinct()
                .ToList();

            updates.Add(new PointDocument
            {
                Id = point.Id,
                Title = point.Title,
                Latitude = point.Latitude,
                Longitude = point.Longitude,
                AddressRegion = point.AddressRegion,
                DescriptionText = point.DescriptionText,
                CollectionIds = remainingPointCollectionIds,
                PrimaryCollectionId = PointCollectionRules.RemoveCollection(
                    remainingPointCollectionIds,
                    removedCollectionId,
                    point.PrimaryCollectionId,
                    remainingCollections),
                TagsText = point.TagsText,
                Photos = point.Photos.ToList(),
                Files = point.Files.ToList(),
            });
        }

        return updates;
    }

    private static IReadOnlyList<string> BuildLoadErrors(
        IEnumerable<FileLibraryLoadError> collectionErrors,
        IEnumerable<FileLibraryLoadError> pointErrors)
    {
        return collectionErrors
            .Select(error => $"Collection JSON could not be loaded: {Path.GetFileName(error.FilePath)}. Reason: {error.Message}")
            .Concat(pointErrors.Select(error => $"Point JSON could not be loaded: {Path.GetFileName(error.FilePath)}. Reason: {error.Message}"))
            .ToArray();
    }

    private static IReadOnlyList<string> BuildPointUpdateFailure(
        string collectionName,
        string pointTitle,
        string reason) =>
        [
            $"Cannot delete collection \"{collectionName}\".",
            $"Point \"{pointTitle}\" could not be updated.",
            $"Reason: {reason}",
            "Fix: close programs using ChoKuda files and try again. If the point JSON is broken, repair or remove it manually.",
        ];

    private static IReadOnlyList<string> BuildCollectionDeleteFailure(
        string collectionName,
        string reason) =>
        [
            $"Cannot delete collection \"{collectionName}\".",
            $"Reason: {reason}",
            "Fix: close programs using ChoKuda files and try again.",
        ];

    private static string NormalizeName(string name) =>
        string.Join(' ', name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .ToLowerInvariant();
}
