namespace ChoKuda.Core.Filters;

public sealed record PointFilterOptions(
    IReadOnlyList<Guid> CollectionIds,
    FilterMode CollectionMode,
    IReadOnlyList<string> Tags,
    FilterMode TagMode);
