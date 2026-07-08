using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Attachments;

public sealed record AttachmentDeleteResult(
    PointDocument Point,
    IReadOnlyList<string> Errors)
{
    public bool IsSuccess => Errors.Count == 0;

    public static AttachmentDeleteResult Success(PointDocument point) =>
        new(point, Array.Empty<string>());

    public static AttachmentDeleteResult Failure(PointDocument point, IEnumerable<string> errors) =>
        new(point, errors.ToArray());
}
