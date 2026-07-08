using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Attachments;

public sealed record AttachmentImportResult(
    PointDocument Point,
    IReadOnlyList<AttachmentImportError> Errors);
