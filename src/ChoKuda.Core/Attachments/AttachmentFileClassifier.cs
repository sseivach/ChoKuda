namespace ChoKuda.Core.Attachments;

public sealed class AttachmentFileClassifier
{
    private static readonly HashSet<string> PhotoCandidateExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
            ".gif",
        };

    private readonly IImageProbe _imageProbe;

    public AttachmentFileClassifier(IImageProbe imageProbe)
    {
        _imageProbe = imageProbe;
    }

    public AttachmentKind Classify(string filePath)
    {
        var extension = Path.GetExtension(filePath);

        if (!PhotoCandidateExtensions.Contains(extension))
        {
            return AttachmentKind.File;
        }

        return _imageProbe.CanDecodeImage(filePath)
            ? AttachmentKind.Photo
            : AttachmentKind.File;
    }
}
