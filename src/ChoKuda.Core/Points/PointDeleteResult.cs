namespace ChoKuda.Core.Points;

public sealed record PointDeleteResult(IReadOnlyList<string> Errors)
{
    public bool IsSuccess => Errors.Count == 0;

    public static PointDeleteResult Success() =>
        new(Array.Empty<string>());

    public static PointDeleteResult Failure(IEnumerable<string> errors) =>
        new(errors.ToArray());
}
