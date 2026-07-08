using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Attachments;

public sealed class AttachmentDeleteService
{
    private readonly IAttachmentFileSystem _fileSystem;
    private readonly FileLibraryService _fileLibraryService;

    public AttachmentDeleteService()
        : this(new PhysicalAttachmentFileSystem(), new FileLibraryService())
    {
    }

    public AttachmentDeleteService(
        IAttachmentFileSystem fileSystem,
        FileLibraryService fileLibraryService)
    {
        _fileSystem = fileSystem;
        _fileLibraryService = fileLibraryService;
    }

    public AttachmentDeleteResult DeleteSavedAttachment(
        FileLibraryPaths paths,
        PointDocument point,
        AttachmentKind kind,
        string storedName)
    {
        if (string.IsNullOrWhiteSpace(storedName))
        {
            return AttachmentDeleteResult.Failure(point, ["Attachment file name is required."]);
        }

        if (Path.GetFileName(storedName) != storedName)
        {
            return AttachmentDeleteResult.Failure(point, [$"Attachment file name is invalid: {storedName}"]);
        }

        var directoryPath = kind == AttachmentKind.Photo
            ? paths.PhotosPath
            : paths.FilesPath;
        var filePath = Path.Combine(directoryPath, storedName);

        try
        {
            if (_fileSystem.FileExists(filePath))
            {
                _fileSystem.DeleteFile(filePath);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return AttachmentDeleteResult.Failure(point, [$"Attachment could not be deleted: {storedName}. {exception.Message}"]);
        }

        var updatedPoint = ClonePoint(point);

        if (kind == AttachmentKind.Photo)
        {
            updatedPoint.Photos.Remove(storedName);
        }
        else
        {
            updatedPoint.Files.Remove(storedName);
        }

        _fileLibraryService.SavePoint(paths, updatedPoint);

        return AttachmentDeleteResult.Success(updatedPoint);
    }

    private static PointDocument ClonePoint(PointDocument point) =>
        new()
        {
            Id = point.Id,
            Title = point.Title,
            Latitude = point.Latitude,
            Longitude = point.Longitude,
            AddressRegion = point.AddressRegion,
            DescriptionText = point.DescriptionText,
            CollectionIds = point.CollectionIds.ToList(),
            PrimaryCollectionId = point.PrimaryCollectionId,
            TagsText = point.TagsText,
            Photos = point.Photos.ToList(),
            Files = point.Files.ToList(),
        };
}
