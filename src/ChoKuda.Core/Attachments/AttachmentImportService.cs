using ChoKuda.Core.Domain;
using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Attachments;

public sealed class AttachmentImportService
{
    private readonly IAttachmentFileSystem _fileSystem;

    public AttachmentImportService()
        : this(new PhysicalAttachmentFileSystem())
    {
    }

    public AttachmentImportService(IAttachmentFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public AttachmentImportResult ImportPendingAttachments(
        FileLibraryPaths paths,
        PointDocument point,
        IEnumerable<PendingAttachment> pendingAttachments)
    {
        var updatedPoint = ClonePoint(point);
        var errors = new List<AttachmentImportError>();

        foreach (var attachment in pendingAttachments)
        {
            var sourcePath = attachment.SourcePath;

            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                errors.Add(new AttachmentImportError(sourcePath, "Source path is empty."));
                continue;
            }

            if (!_fileSystem.FileExists(sourcePath))
            {
                errors.Add(new AttachmentImportError(sourcePath, "Source file was not found."));
                continue;
            }

            var storedName = AttachmentFileName.CreateStoredName(
                Path.GetFileName(sourcePath),
                Guid.NewGuid());
            var destinationDirectory = attachment.Kind == AttachmentKind.Photo
                ? paths.PhotosPath
                : paths.FilesPath;
            var destinationPath = Path.Combine(destinationDirectory, storedName);

            try
            {
                _fileSystem.CopyFile(sourcePath, destinationPath);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                errors.Add(new AttachmentImportError(sourcePath, $"File could not be copied: {exception.Message}"));
                continue;
            }

            if (attachment.Kind == AttachmentKind.Photo)
            {
                updatedPoint.Photos.Add(storedName);
            }
            else
            {
                updatedPoint.Files.Add(storedName);
            }
        }

        return new AttachmentImportResult(updatedPoint, errors);
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
