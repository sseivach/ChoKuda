namespace ChoKuda.Core.Collections;

public sealed record CollectionDeleteResult(IReadOnlyList<string> Errors)
{
    public bool IsSuccess => Errors.Count == 0;

    public static CollectionDeleteResult Success() =>
        new(Array.Empty<string>());

    public static CollectionDeleteResult Failure(IEnumerable<string> errors) =>
        new(errors.ToArray());
}
