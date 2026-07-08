using ChoKuda.Core.Domain;
using ChoKuda.Core.FileLibrary;

namespace ChoKuda.Core.Filters;

public static class PointFilterService
{
    public static IReadOnlyList<PointDocument> Apply(
        IEnumerable<PointDocument> points,
        PointFilterOptions options)
    {
        return points
            .Where(point => MatchesCollections(point, options.CollectionIds, options.CollectionMode))
            .Where(point => MatchesTags(point, options.Tags, options.TagMode))
            .ToArray();
    }

    public static bool MatchesCollections(
        PointDocument point,
        IReadOnlyCollection<Guid> selectedCollectionIds,
        FilterMode mode)
    {
        if (selectedCollectionIds.Count == 0)
        {
            return true;
        }

        return mode == FilterMode.All
            ? selectedCollectionIds.All(point.CollectionIds.Contains)
            : selectedCollectionIds.Any(point.CollectionIds.Contains);
    }

    public static bool MatchesTags(
        PointDocument point,
        IReadOnlyCollection<string> selectedTags,
        FilterMode mode)
    {
        if (selectedTags.Count == 0)
        {
            return true;
        }

        var pointTags = TagNormalization.Normalize(point.TagsText).NormalizedText
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.Ordinal);
        var normalizedSelectedTags = selectedTags
            .Select(tag => TagNormalization.Normalize(tag).NormalizedText)
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .ToArray();

        return mode == FilterMode.All
            ? normalizedSelectedTags.All(pointTags.Contains)
            : normalizedSelectedTags.Any(pointTags.Contains);
    }
}
