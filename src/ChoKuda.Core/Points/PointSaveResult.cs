using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Points;

public sealed record PointSaveResult(
    PointDocument? Point,
    IReadOnlyList<PointSaveError> Errors)
{
    public bool IsSuccess => Errors.Count == 0;

    public static PointSaveResult Success(PointDocument point) =>
        new(point, Array.Empty<PointSaveError>());

    public static PointSaveResult Failure(IEnumerable<PointSaveError> errors) =>
        new(null, errors.ToArray());
}
