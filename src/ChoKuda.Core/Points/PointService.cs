using System.Text.Json;
using ChoKuda.Core.Domain;
using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Points;

public sealed class PointService
{
    public const string TitleFieldName = "title";
    public const string TagsFieldName = "tags";
    public const string GeneralFieldName = "general";

    private readonly FileLibraryService _fileLibraryService;
    private readonly IPointFileSystem _fileSystem;

    public PointService()
        : this(new FileLibraryService(), new PhysicalPointFileSystem())
    {
    }

    public PointService(
        FileLibraryService fileLibraryService,
        IPointFileSystem fileSystem)
    {
        _fileLibraryService = fileLibraryService;
        _fileSystem = fileSystem;
    }

    public PointDocument CreateDraft(double latitude, double longitude) =>
        new()
        {
            Title = "New point",
            Latitude = latitude,
            Longitude = longitude,
        };

    public PointDocument? LoadPoint(FileLibraryPaths paths, Guid pointId)
    {
        var filePath = paths.GetPointFilePath(pointId);

        if (!_fileSystem.FileExists(filePath))
        {
            return null;
        }

        return _fileLibraryService.LoadJson<PointDocument>(filePath);
    }

    public PointSaveResult CreatePoint(FileLibraryPaths paths, PointDocument draft)
    {
        var point = CopyAndNormalize(draft);
        point.Id = Guid.NewGuid();

        return SaveValidatedPoint(paths, point);
    }

    public PointSaveResult UpdatePoint(FileLibraryPaths paths, PointDocument point)
    {
        var normalizedPoint = CopyAndNormalize(point);

        return SaveValidatedPoint(paths, normalizedPoint);
    }

    public PointDeleteResult DeletePoint(FileLibraryPaths paths, Guid pointId)
    {
        if (pointId == Guid.Empty)
        {
            return PointDeleteResult.Failure(["Point id is required."]);
        }

        var pointFilePath = paths.GetPointFilePath(pointId);

        if (!_fileSystem.FileExists(pointFilePath))
        {
            return PointDeleteResult.Failure(["Point JSON was not found."]);
        }

        PointDocument point;
        try
        {
            point = _fileLibraryService.LoadJson<PointDocument>(pointFilePath);
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException or InvalidDataException)
        {
            return PointDeleteResult.Failure([$"Point JSON could not be loaded: {exception.Message}"]);
        }

        var attachmentErrors = DeleteAttachments(paths, point);

        if (attachmentErrors.Count > 0)
        {
            return PointDeleteResult.Failure(attachmentErrors);
        }

        try
        {
            _fileSystem.DeleteFile(pointFilePath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return PointDeleteResult.Failure([$"Point JSON could not be deleted: {exception.Message}"]);
        }

        return PointDeleteResult.Success();
    }

    private PointSaveResult SaveValidatedPoint(FileLibraryPaths paths, PointDocument point)
    {
        var errors = ValidatePoint(point);

        if (errors.Count > 0)
        {
            return PointSaveResult.Failure(errors);
        }

        _fileLibraryService.SavePoint(paths, point);

        return PointSaveResult.Success(point);
    }

    private static IReadOnlyList<PointSaveError> ValidatePoint(PointDocument point)
    {
        var errors = new List<PointSaveError>();

        if (string.IsNullOrWhiteSpace(point.Title))
        {
            errors.Add(new PointSaveError(TitleFieldName, "Title is required."));
        }

        var requiredFieldErrors = PointValidation.ValidateRequiredFields(
            point.Id,
            point.Title,
            point.Latitude,
            point.Longitude);

        errors.AddRange(
            requiredFieldErrors
                .Where(error => error != "Point title is required.")
                .Select(error => new PointSaveError(GeneralFieldName, error)));

        var tagResult = TagNormalization.Normalize(point.TagsText);

        if (!tagResult.IsValid)
        {
            errors.Add(new PointSaveError(
                TagsFieldName,
                $"Every tag must start with #: {string.Join(", ", tagResult.InvalidTokens)}"));
        }

        return errors;
    }

    private static PointDocument CopyAndNormalize(PointDocument point)
    {
        var tagResult = TagNormalization.Normalize(point.TagsText);

        return new PointDocument
        {
            Id = point.Id,
            Title = point.Title.Trim(),
            Latitude = point.Latitude,
            Longitude = point.Longitude,
            AddressRegion = point.AddressRegion.Trim(),
            DescriptionText = point.DescriptionText.Trim(),
            CollectionIds = point.CollectionIds.ToList(),
            PrimaryCollectionId = point.CollectionIds.Count == 0
                ? null
                : point.PrimaryCollectionId,
            TagsText = tagResult.IsValid
                ? tagResult.NormalizedText
                : point.TagsText.Trim(),
            Photos = point.Photos.ToList(),
            Files = point.Files.ToList(),
        };
    }

    private IReadOnlyList<string> DeleteAttachments(FileLibraryPaths paths, PointDocument point)
    {
        var errors = new List<string>();

        DeleteAttachmentList(paths.PhotosPath, point.Photos, "photo", errors);
        DeleteAttachmentList(paths.FilesPath, point.Files, "file", errors);

        return errors;
    }

    private void DeleteAttachmentList(
        string directoryPath,
        IEnumerable<string> fileNames,
        string kind,
        ICollection<string> errors)
    {
        foreach (var fileName in fileNames)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                errors.Add($"Linked {kind} has an empty file name.");
                continue;
            }

            if (Path.GetFileName(fileName) != fileName)
            {
                errors.Add($"Linked {kind} has an invalid file name: {fileName}");
                continue;
            }

            var filePath = Path.Combine(directoryPath, fileName);

            if (!_fileSystem.FileExists(filePath))
            {
                continue;
            }

            try
            {
                _fileSystem.DeleteFile(filePath);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                errors.Add($"Linked {kind} could not be deleted: {fileName}. {exception.Message}");
            }
        }
    }
}
