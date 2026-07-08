using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Collections;

public sealed record CollectionSaveResult(
    CollectionDocument? Collection,
    IReadOnlyList<CollectionSaveError> Errors)
{
    public bool IsSuccess => Errors.Count == 0;

    public static CollectionSaveResult Success(CollectionDocument collection) =>
        new(collection, Array.Empty<CollectionSaveError>());

    public static CollectionSaveResult Failure(IEnumerable<CollectionSaveError> errors) =>
        new(null, errors.ToArray());
}
